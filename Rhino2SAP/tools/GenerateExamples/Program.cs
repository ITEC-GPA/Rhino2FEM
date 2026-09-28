using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI.Canvas;
using Rhino.Runtime.InProcess;
using Rhino.Geometry;
using Rhino2SAP.Core;
using System.Drawing;

internal static class Program
{
    [STAThread] static int Main(string[] args)
    {
        RhinoInside.Resolver.Initialize(@"C:\Program Files\Rhino 8\System");
        return Run(args.Length==0?Environment.CurrentDirectory:Path.GetFullPath(args[0]),args.Length>1?Path.GetFullPath(args[1]):null);
    }
    [MethodImpl(MethodImplOptions.NoInlining)] static int Run(string root,string? output)
    {
        try
        {
            using var rhino=new RhinoCore(["/nosplash"],WindowStyle.NoWindow);
            string plugin=Path.Combine(root,"Rhino2SAP.Grasshopper","bin","Release","net8.0-windows","Rhino2SAP.Grasshopper.gha");
            AssemblyLoadContext.Default.Resolving+=(context,name)=>{string file=Path.Combine(Path.GetDirectoryName(plugin)!,name.Name+".dll");return File.Exists(file)?context.LoadFromAssemblyPath(file):null;};
            var assembly=Assembly.LoadFrom(plugin);
            // Use a private in-process catalogue so unrelated installed GH plugins are not started.
            var server=new GH_ComponentServer();
            typeof(global::Grasshopper.Instances).GetField("m_comServer",BindingFlags.Static|BindingFlags.NonPublic)!.SetValue(null,server);
            var load=server.GetType().GetMethod("LoadGHA",BindingFlags.Instance|BindingFlags.NonPublic)??throw new MissingMethodException("Grasshopper LoadGHA");
            var coreInfo=new GH_AssemblyInfoStub(typeof(GH_Component).Assembly);server.AddProxyLibraryInfo(coreInfo);
            var proxyType=typeof(GH_Component).Assembly.GetType("Grasshopper.Kernel.GH_CompiledObjectProxy",true)!;
            foreach(var item in new IGH_DocumentObject[]{new GH_Panel(),new GH_Group(),new GH_NumberSlider(),new GH_BooleanToggle(),new Param_Brep(),new Param_Mesh()})
                server.AddProxy((IGH_ObjectProxy)Activator.CreateInstance(proxyType,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,[item,coreInfo],null)!);
            if(!(bool)load.Invoke(server,[new GH_ExternalFile(plugin),false])!)throw new Exception("Grasshopper could not register the plugin.");
            Console.WriteLine("Registered Grasshopper built-ins and Rhino2SAP only.");
            string directory=output??Path.Combine(root,".local","examples-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
            var report=new List<object>();
            foreach(var example in Examples.Create(assembly))
            {
                using var doc=example.Graph.Document;
                foreach(var obj in doc.Objects)
                    if(global::Grasshopper.Instances.ComponentServer.EmitObject(obj.ComponentGuid)==null)throw new Exception("Unregistered component: "+obj.Name);
                doc.Enabled=true;doc.NewSolution(false,GH_SolutionMode.Silent);
                Verify(doc,example.ExpectedBreps);
                int objectCount=doc.Objects.Count,wireCount=WireCount(doc);
                foreach(string extension in new[]{"gh","ghx"})
                {
                    string file=Path.Combine(directory,example.Name+"."+extension);
                    var io=new GH_DocumentIO(doc);if(!io.SaveQuiet(file))throw new Exception("Could not save "+file);
                    var opened=new GH_DocumentIO();if(!opened.Open(file))throw new Exception("Could not reopen "+file);
                    using var restored=opened.Document;
                    if(restored.Objects.Count!=objectCount||WireCount(restored)!=wireCount)throw new Exception("Round trip lost objects/wires in "+file);
                    restored.Enabled=true;restored.NewSolution(false,GH_SolutionMode.Silent);Verify(restored,example.ExpectedBreps);
                    Console.WriteLine($"PASS {Path.GetFileName(file)}: reopened, {objectCount} objects, {wireCount} wires, {example.ExpectedBreps} closed Breps, Run/Bake=false.");
                }
                Render(doc,Path.Combine(directory,example.Name+".png"));
                report.Add(new{File=example.Name+".gh",Objects=objectCount,Wires=wireCount,SapComponents=doc.Objects.OfType<GH_Component>().Count(c=>c.Category=="SAP2000"),ClosedBreps=example.ExpectedBreps,RoundTripGh=true,RoundTripGhx=true,AnalysisExecuted=false});
            }
            File.WriteAllText(Path.Combine(directory,"validation.json"),JsonSerializer.Serialize(new{GeneratedUtc=DateTimeOffset.UtcNow,Examples=report},new JsonSerializerOptions{WriteIndented=true}));
            Console.WriteLine("Completed examples and canvas previews: "+directory);
            return 0;
        }
        catch(Exception ex){Console.WriteLine(ex);return 1;}
    }
    static int WireCount(GH_Document doc)=>doc.Objects.OfType<IGH_Param>().Sum(p=>p.SourceCount)+doc.Objects.OfType<GH_Component>().Sum(c=>c.Params.Input.Sum(p=>p.SourceCount));
    static void Verify(GH_Document doc,int expectedBreps)
    {
        foreach(var toggle in doc.Objects.OfType<GH_BooleanToggle>())if(toggle.NickName is "RUN SAP" or "BAKE SOLIDI"&&toggle.Value)throw new Exception("Action trigger unexpectedly true.");
        var components=doc.Objects.OfType<GH_Component>().ToArray();
        var errors=components.SelectMany(c=>c.RuntimeMessages(GH_RuntimeMessageLevel.Error).Select(e=>c.Name+": "+e)).ToArray();
        if(errors.Length>0)throw new Exception(string.Join("\n",errors));
        var model=components.Single(c=>c.GetType().Name=="BuildModelComponent").Params.Output[0].VolatileData.AllData(true).Single().ScriptVariable() as SapModel??throw new Exception("No Model output.");
        ModelValidation.RequireValid(model);
        var preview=components.Single(c=>c.GetType().Name=="PreviewModelComponent");
        var breps=preview.Params.Output[2].VolatileData.AllData(true).OfType<GH_Brep>().Select(b=>b.Value).ToArray();
        if(breps.Length!=expectedBreps||breps.Any(b=>!b.IsValid||!b.IsSolid))throw new Exception("Invalid or missing physical Breps.");
        if(components.Any(c=>c.GetType().Name=="AnalyzeAndEmbedComponent"&&c.Params.Output[0].VolatileData.AllData(true).Any(v=>v?.ScriptVariable()!=null)))throw new Exception("Unexpected analysis results.");
        foreach(var component in components.Where(c=>c.GetType().Name=="BakeGeometryComponent"))if(component.Params.Output[0].VolatileData.AllData(true).Any(v=>v?.ScriptVariable()!=null))throw new Exception("Unexpected baked objects.");
    }
    static void Render(GH_Document doc,string path)
    {
        foreach(var obj in doc.Objects){obj.Attributes.ExpireLayout();obj.Attributes.PerformLayout();}
        var bounds=doc.Objects.Select(o=>o.Attributes.Bounds).Aggregate(RectangleF.Union);bounds.Inflate(45,45);
        using var canvas=new GH_Canvas{Document=doc,Size=new Size((int)Math.Ceiling(bounds.Width),(int)Math.Ceiling(bounds.Height))};
        var viewport=new GH_Viewport(new System.Drawing.Point((int)-bounds.Left,(int)-bounds.Top),1f){Size=canvas.Size};
        using var bitmap=canvas.GenerateHiResImageTile(viewport,Color.FromArgb(248,249,251));bitmap.Save(path,System.Drawing.Imaging.ImageFormat.Png);
    }
}
