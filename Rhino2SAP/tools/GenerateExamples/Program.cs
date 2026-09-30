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
            string plugin=Path.Combine(root,"bin","Rhino2SAP.Grasshopper.gha");
            AssemblyLoadContext.Default.Resolving+=(context,name)=>{string file=Path.Combine(Path.GetDirectoryName(plugin)!,name.Name+".dll");return File.Exists(file)?context.LoadFromAssemblyPath(file):null;};
            var assembly=Assembly.LoadFrom(plugin);
            // Use a private in-process catalogue so unrelated installed GH plugins are not started.
            var server=new GH_ComponentServer();
            typeof(global::Grasshopper.Instances).GetField("m_comServer",BindingFlags.Static|BindingFlags.NonPublic)!.SetValue(null,server);
            var load=server.GetType().GetMethod("LoadGHA",BindingFlags.Instance|BindingFlags.NonPublic)??throw new MissingMethodException("Grasshopper LoadGHA");
            var coreInfo=new GH_AssemblyInfoStub(typeof(GH_Component).Assembly);server.AddProxyLibraryInfo(coreInfo);
            var proxyType=typeof(GH_Component).Assembly.GetType("Grasshopper.Kernel.GH_CompiledObjectProxy",true)!;
            foreach(var item in new IGH_DocumentObject[]{new GH_Panel(),new GH_Group(),new GH_NumberSlider(),new GH_BooleanToggle(),new Param_Brep(),new Param_Mesh(),new Param_Curve(),new Param_Point(),new Param_Line(),new Param_Vector(),new Param_Plane()})
                server.AddProxy((IGH_ObjectProxy)Activator.CreateInstance(proxyType,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,[item,coreInfo],null)!);
            if(!(bool)load.Invoke(server,[new GH_ExternalFile(plugin),false])!)throw new Exception("Grasshopper could not register the plugin.");
            Console.WriteLine("Registered Grasshopper built-ins and Rhino2SAP only.");
            string directory=output??Path.Combine(root,".local","examples-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
            var report=new List<ExampleReport>();
            foreach(var example in Examples.Create(assembly))
            {
                Console.WriteLine("Generating "+example.Name);
                using var doc=example.Graph.Document;
                foreach(var obj in doc.Objects)
                    if(global::Grasshopper.Instances.ComponentServer.EmitObject(obj.ComponentGuid)==null)throw new Exception("Unregistered component: "+obj.Name);
                doc.Enabled=true;doc.NewSolution(false,GH_SolutionMode.Silent);
                Verify(doc,example);
                var fingerprints=Fingerprints(doc);
                int objectCount=doc.Objects.Count,wireCount=WireCount(doc);
                foreach(string extension in new[]{"gh","ghx"})
                {
                    string file=Path.Combine(directory,example.Name+"."+extension);
                    var io=new GH_DocumentIO(doc);if(!io.SaveQuiet(file))throw new Exception("Could not save "+file);
                    var opened=new GH_DocumentIO();if(!opened.Open(file))throw new Exception("Could not reopen "+file);
                    using var restored=opened.Document;
                    if(restored.Objects.Count!=objectCount||WireCount(restored)!=wireCount)throw new Exception("Round trip lost objects/wires in "+file);
                    restored.Enabled=true;restored.NewSolution(false,GH_SolutionMode.Silent);Verify(restored,example);
                    if(!fingerprints.SequenceEqual(Fingerprints(restored)))throw new Exception("Round trip changed the Model definitions in "+file);
                    Console.WriteLine($"PASS {Path.GetFileName(file)}: reopened, {objectCount} objects, {wireCount} wires, {example.ExpectedBreps} closed Breps, Run/Bake=false.");
                }
                Render(doc,Path.Combine(directory,example.Name.ToLowerInvariant()+".png"));
                var components=doc.Objects.OfType<GH_Component>().Where(c=>c.Category=="SAP2000").ToArray();
                report.Add(new(example.Name+".gh",example.Description,example.Mode,objectCount,wireCount,components.Length,
                    example.ExpectedBreps,components.Select(c=>c.SubCategory).Distinct().Order().ToArray(),
                    components.Select(c=>c.GetType().Name).Distinct().Order().ToArray(),fingerprints));
                ExampleDocumentation.WriteGuide(directory,example,components);
            }
            var topics=(IReadOnlyList<string>)assembly.GetType("Rhino2SAP.Grasshopper.ComponentTopics")!.GetProperty("All")!.GetValue(null)!;
            var covered=report.SelectMany(r=>r.Topics).ToHashSet();
            var missing=topics.Where(t=>!covered.Contains(t)).ToArray();
            if(missing.Length!=0)throw new Exception("Topics without examples: "+string.Join(", ",missing));
            File.WriteAllText(Path.Combine(directory,"validation.json"),JsonSerializer.Serialize(new{GeneratedUtc=DateTimeOffset.UtcNow,Topics=topics.Count,CoveredTopics=covered.Count,Examples=report},new JsonSerializerOptions{WriteIndented=true}));
            ExampleDocumentation.WriteIndex(directory,report,topics);
            Console.WriteLine("Completed examples and canvas previews: "+directory);
            return 0;
        }
        catch(Exception ex){Console.WriteLine(ex);return 1;}
    }
    static int WireCount(GH_Document doc)=>doc.Objects.OfType<IGH_Param>().Sum(p=>p.SourceCount)+doc.Objects.OfType<GH_Component>().Sum(c=>c.Params.Input.Sum(p=>p.SourceCount));
    static string[] Fingerprints(GH_Document doc)=>doc.Objects.OfType<GH_Component>().SelectMany(c=>c.Params.Output)
        .SelectMany(p=>p.VolatileData.AllData(true)).Select(v=>v?.ScriptVariable()).OfType<SapModel>()
        .Select(m=>m.Fingerprint).Distinct().Order().ToArray();
    static void Verify(GH_Document doc,Example example)
    {
        foreach(var toggle in doc.Objects.OfType<GH_BooleanToggle>())if(toggle.NickName is "RUN SAP" or "BAKE SOLIDI"&&toggle.Value)throw new Exception("Action trigger unexpectedly true.");
        var components=doc.Objects.OfType<GH_Component>().ToArray();
        var errors=components.SelectMany(c=>c.RuntimeMessages(GH_RuntimeMessageLevel.Error).Select(e=>c.Name+": "+e)).ToArray();
        if(errors.Length>0)throw new Exception(string.Join("\n",errors));
        foreach(var component in components)
        {
            foreach(var input in component.Params.Input.Where(p=>!p.Optional&&p.SourceCount==0&&p.VolatileDataCount==0))
                throw new Exception(component.Name+": required input has no data or wire: "+input.Name);
            foreach(var input in component.Params.Input.Where(p=>p.Name is "Run" or "Bake"))
                if(input.VolatileData.AllData(true).OfType<GH_Boolean>().Any(v=>v.Value))throw new Exception(component.Name+": action enabled.");
            if(component.Params.Input.Any(p=>p.Name is "Run" or "Bake")&&component.Params.Output.SelectMany(p=>p.VolatileData.AllData(true)).Any(v=>v?.ScriptVariable()!=null))
                throw new Exception("An action published data before a trigger: "+component.Name);
        }
        var models=components.SelectMany(c=>c.Params.Output).SelectMany(p=>p.VolatileData.AllData(true)).Select(v=>v?.ScriptVariable()).OfType<SapModel>().ToArray();
        if(example.RequiresModel&&models.Length==0)throw new Exception("No Model output.");
        foreach(var model in models)ModelValidation.RequireValid(model);
        var breps=components.Where(c=>c.GetType().Name=="PreviewModelComponent").SelectMany(c=>c.Params.Output[2].VolatileData.AllData(true)).OfType<GH_Brep>().Select(b=>b.Value).ToArray();
        if(breps.Length!=example.ExpectedBreps||breps.Any(b=>!b.IsValid||!b.IsSolid))throw new Exception($"Invalid physical Breps: expected {example.ExpectedBreps}, got {breps.Length}.");
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
