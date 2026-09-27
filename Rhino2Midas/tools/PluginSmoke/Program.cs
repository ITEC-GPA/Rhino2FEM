using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Runtime.InProcess;
using Rhino2Midas.Core;

internal static class Program
{
    static void Progress(string message){Console.WriteLine(message);File.AppendAllText(Path.Combine(Environment.CurrentDirectory,"docs","rhino-smoke.log"),message+Environment.NewLine);}
    [STAThread] static int Main(){File.WriteAllText(Path.Combine(Environment.CurrentDirectory,"docs","rhino-smoke.log"),DateTimeOffset.Now+Environment.NewLine);Progress("Starting Rhino.Inside resolver");RhinoInside.Resolver.Initialize(@"C:\Program Files\Rhino 8\System");return Run();}
    [MethodImpl(MethodImplOptions.NoInlining)] static int Run()
    {
        try
        {
            Progress("Starting RhinoCore");using var rhino=new RhinoCore(["/nosplash"],WindowStyle.NoWindow);Progress("RhinoCore ready");
            string root=Environment.CurrentDirectory;
            string plugin=Path.Combine(root,"Rhino2Midas.Grasshopper","bin","Release","net8.0-windows","Rhino2Midas.Grasshopper.gha");
            AssemblyLoadContext.Default.Resolving+=(context,name)=>{string file=Path.Combine(Path.GetDirectoryName(plugin)!,name.Name+".dll");return File.Exists(file)?context.LoadFromAssemblyPath(file):null;};
            var assembly=Assembly.LoadFrom(plugin);var ids=new HashSet<Guid>();var catalogue=new List<object>();var components=new List<GH_Component>();

            foreach(var type in assembly.GetTypes().Where(t=>!t.IsAbstract&&typeof(GH_Component).IsAssignableFrom(t)).OrderBy(t=>t.Name))
            {

                var c=(GH_Component)Activator.CreateInstance(type)!;
                Check(ids.Add(c.ComponentGuid),"Unique component GUID: "+type.Name);
                Check(c.Icon_24x24!=null&&c.Icon_24x24.Width==24&&c.Params.Output.Count>0,"Icon and parameters: "+type.Name);
                components.Add(c);
                catalogue.Add(new{Class=type.Name,c.Name,Category=c.SubCategory,Guid=c.ComponentGuid,Inputs=c.Params.Input.Select(p=>new{p.Name,p.Description,p.Optional,Access=p.Access.ToString()}),Outputs=c.Params.Output.Select(p=>new{p.Name,p.Description,Access=p.Access.ToString()})});
            }
            File.WriteAllText(Path.Combine(root,"docs","components.json"),JsonSerializer.Serialize(catalogue,new JsonSerializerOptions{WriteIndented=true}));
            File.WriteAllText(Path.Combine(root,"docs","COMPONENTS.md"),"# Componenti Rhino2Midas\n\n"+components.Count+" componenti; nomi e GUID stabili. Gli ingressi avanzati seguono gli esempi ufficiali, da adattare al modello.\n\n"+string.Join("\n\n",components.GroupBy(c=>c.SubCategory).OrderBy(g=>g.Key).Select(g=>"## "+g.Key+"\n\n"+string.Join("\n",g.Select(c=>"- "+c.Name+" (`"+c.ComponentGuid+"`)")))));
            Console.WriteLine($"PASS: {components.Count} components instantiate in Rhino with unique GUIDs, icons and sockets.");
            using(var gallery=new Bitmap(1200,((components.Count+3)/4)*42+20))
            using(var graphics=Graphics.FromImage(gallery))
            using(var font=new Font("Segoe UI",8))
            {graphics.Clear(Color.White);for(int i=0;i<components.Count;i++){int x=(i%4)*300+8,y=(i/4)*42+8;graphics.DrawImage(components[i].Icon_24x24,x,y);graphics.DrawString(components[i].Name,font,Brushes.Black,new RectangleF(x+30,y,260,36));}SavePng(gallery,Path.Combine(root,"docs","component-icons.png"));}
            using var doc=new GH_Document();
            GH_Component Create(string name){var c=(GH_Component)Activator.CreateInstance(assembly.GetType("Rhino2Midas.Grasshopper."+name)!)!;doc.AddObject(c,false);return c;}
            void Solve(GH_Component c){c.CollectData();c.ComputeData();Check(c.RuntimeMessages(GH_RuntimeMessageLevel.Error).Count==0,c.Name+": "+string.Join("; ",c.RuntimeMessages(GH_RuntimeMessageLevel.Error)));}
            var nativeFailures=new List<string>();int nativeSolved=0;
            foreach(var c in components.Where(c=>c.GetType().Name.StartsWith("Api_")||c.GetType().Name.StartsWith("Request_")))
            {doc.AddObject(c,false);try{Solve(c);nativeSolved++;}catch(Exception ex){nativeFailures.Add(ex.Message);}doc.RemoveObject(c,false);}
            Check(nativeFailures.Count==0,"Official API example components fail: "+string.Join("\n",nativeFailures));
            Console.WriteLine($"PASS: {nativeSolved} native definition/request components solve from their documented examples.");
            var gooType=assembly.GetType("Rhino2Midas.Grasshopper.ModelGoo")!;
            void SetModel(GH_Component c,MidasModel m,int input=0)=>((Param_GenericObject)c.Params.Input[input]).PersistentData.Append((IGH_Goo)Activator.CreateInstance(gooType,m)!);
            var example=Create("ExampleModelComponent");Solve(example);Check(example.Params.Output[0].VolatileDataCount==1,"Example Model solves");
            var build=Create("BuildModelComponent");build.Params.Input[0].AddSource(example.Params.Output[0]);Solve(build);
            var physical=Create("PhysicalGeometryComponent");physical.Params.Input[0].AddSource(example.Params.Output[0]);Solve(physical);
            var solids=physical.Params.Output[0].VolatileData.AllData(true).Select(g=>g.ScriptVariable()).OfType<Brep>().ToArray();
            Check(solids.Length==1&&solids[0].IsSolid&&Math.Abs(VolumeMassProperties.Compute(solids[0]).Volume-.18)<1e-7,"Cantilever volume 0.18 m³");
            Console.WriteLine("PASS: Example → Build Model → physical geometry through Grasshopper wires (closed 0.18 m³ beam).");
            var decompose=Create("DecomposeModelComponent");decompose.Params.Input[0].AddSource(example.Params.Output[0]);Solve(decompose);
            var selected=(Fragment)decompose.Params.Output[1].VolatileData.AllData(true).Single().ScriptVariable();
            Check(ModelValidation.Errors(new MidasModel(selected)).Count==0,"Decomposed beam can rebuild with native connectivity");
            var brepParam=new Param_Brep();doc.AddObject(brepParam,false);brepParam.AddSource(decompose.Params.Output[1]);brepParam.CollectData();brepParam.ComputeData();
            Check(brepParam.VolatileDataCount==1&&brepParam.VolatileData.AllData(true).Single().ScriptVariable() is Brep,"Element casts to Brep");
            var display=Create("DisplaySettingsComponent");Solve(display);var preview=Create("PreviewModelComponent");preview.Params.Input[0].AddSource(example.Params.Output[0]);preview.Params.Input[1].AddSource(display.Params.Output[0]);Solve(preview);Check(preview.Params.Output[2].VolatileDataCount==1,"Preview Breps output");
            Check((bool)preview.GetType().GetMethod("Manages")!.Invoke(preview,[example])!,"Display Manager identifies upstream plugin components");
            var baseModel=Definitions.Example();var force=new ResultTable("Forces","BEAMFORCE",["Elem","Load","Part","Axial","Shear-y","Shear-z","Torsion","Moment-y","Moment-z"],[["1","LC1","I[1]","0","0","10","0","30","0"],["1","LC1","J[2]","0","0","10","0","0","0"]]);
            var solved=baseModel.WithResults(new(baseModel.Fingerprint,baseModel.Units,"fixture.mcb","fixture",true,[force],[]));
            var resultDecompose=Create("DecomposeModelComponent");SetModel(resultDecompose,solved);Solve(resultDecompose);
            var actions=Create("BeamActionsComponent");actions.Params.Input[0].AddSource(resultDecompose.Params.Output[1]);Solve(actions);Check(actions.Params.Output[4].VolatileData.AllData(true).OfType<GH_Number>().Select(n=>n.Value).SequenceEqual(new[]{30d,0}),"Native moments retained");
            var beamInfo=Create("DecomposeBeamComponent");beamInfo.Params.Input[0].AddSource(resultDecompose.Params.Output[1]);Solve(beamInfo);Check(beamInfo.Params.Output[4].VolatileDataCount==1,"Embedded beam table");
            Console.WriteLine("PASS: solved Model → Decompose → Beam Actions retains case/part rows and native moments.");
            var sceneType=assembly.GetType("Rhino2Midas.Grasshopper.PreviewScene")!;var physicalType=assembly.GetType("Rhino2Midas.Grasshopper.PhysicalGeometry")!;
            (string Shape,double[] D,double Area)[] shapes=[("SB",[.3,.2],.06),("SR",[.2],Math.PI*.01),("P",[.2,.01],Math.PI*(.01-.09*.09)),("B",[.3,.2,.01,.02],.06-.18*.26),("H",[.3,.2,.01,.02,.2,.02],.2*.04+.26*.01),("T",[.3,.2,.01,.02],.2*.02+.28*.01),("C",[.3,.2,.01,.02],.2*.04+.26*.01),("L",[.3,.2,.01,.02],.2*.02+.28*.01)];
            foreach(var shape in shapes)
            {
                var model=new MidasModel(Fragment.Combine([Definitions.Section(1,shape.Shape,shape.Shape,shape.D),new Fragment([new(1,ElementKind.Beam,[new(0,0,0),new(3,0,0)],1,1)])]));
                var scene=sceneType.GetMethod("For",[typeof(MidasModel)])!.Invoke(null,[model])!;using var brep=(Brep?)sceneType.GetMethod("CopySingleBrep")!.Invoke(scene,null);
                Check(brep!=null&&brep.IsSolid&&Math.Abs(VolumeMassProperties.Compute(brep).Volume-shape.Area*3)<1e-7,"Physical "+shape.Shape+" including holes");
            }
            var vertical=new Element(1,ElementKind.Beam,[new(0,0,0),new(0,0,3)]);
            var axes=((Vector3d X,Vector3d Y,Vector3d Z))physicalType.GetMethod("Axes")!.Invoke(null,[vertical])!;Check((axes.Z-Vector3d.XAxis).Length<1e-10,"Native vertical beam beta convention");
            foreach(var specimen in new[]{(new Position[]{new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0)},ElementKind.Plate,.2),(new Position[]{new(0,0,0),new(1,0,0),new(0,1,0),new(0,0,1)},ElementKind.Solid,1d/6),(new Position[]{new(0,0,0),new(1,0,0),new(0,1,0),new(0,0,1),new(1,0,1),new(0,1,1)},ElementKind.Solid,.5),(new Position[]{new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0),new(0,0,1),new(1,0,1),new(1,1,1),new(0,1,1)},ElementKind.Solid,1d)})
            {
                var model=new MidasModel(Fragment.Combine([Definitions.Thickness(1,.2,offset:.1),new Fragment([new(1,specimen.Item2,specimen.Item1,1,1)])]));var scene=sceneType.GetMethod("For",[typeof(MidasModel)])!.Invoke(null,[model])!;using var brep=(Brep?)sceneType.GetMethod("CopySingleBrep")!.Invoke(scene,null);Check(brep!=null&&brep.IsSolid&&Math.Abs(VolumeMassProperties.Compute(brep).Volume-specimen.Item3)<1e-7,"Closed "+specimen.Item2+" "+specimen.Item1.Length+" nodes");
            }
            Console.WriteLine("PASS: all eight physical section profiles, plate thickness, 4/6/8-node solids and vertical beta orientation.");
            // Round-trip the actual plugin goo using Grasshopper's persistence API.
            var goo=(IGH_Goo)Activator.CreateInstance(gooType,solved)!;var archive=new GH_IO.Serialization.GH_Archive();Check(archive.AppendObject(goo,"Model"),"Write GH archive");var restored=(IGH_Goo)Activator.CreateInstance(gooType)!;Check(archive.ExtractObject(restored,"Model"),"Read GH archive");Check(((MidasModel)restored.ScriptVariable()).Results!.Tables.Count==1,"GH archive result persistence");
            using(var window=(Form)Activator.CreateInstance(assembly.GetType("Rhino2Midas.Grasshopper.DisplayWindow")!)!){window.StartPosition=FormStartPosition.Manual;window.Location=new System.Drawing.Point(-32000,-32000);window.Opacity=0;window.ShowInTaskbar=false;window.Show();window.PerformLayout();Application.DoEvents();using var bitmap=new Bitmap(window.Width,window.Height);window.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));SavePng(bitmap,Path.Combine(root,"docs","display-settings.png"));window.Hide();}
            Console.WriteLine("PASS: Grasshopper model/results persistence and display settings window.");
            Directory.CreateDirectory(Path.Combine(root,"examples"));File.WriteAllText(Path.Combine(root,"examples","Cantilever.model.json"),ModelArchive.Serialize(baseModel));
            Console.WriteLine("Native Civil solver is not part of this offline Rhino test.");Progress("SMOKE COMPLETE");return 0;
        }
        catch(Exception ex){Progress(ex.ToString());return 1;}
    }
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    static void SavePng(Image image,string path)
    {
        using var stream=new MemoryStream();image.Save(stream,System.Drawing.Imaging.ImageFormat.Png);
        string temp=Path.Combine(Path.GetDirectoryName(path)!,Guid.NewGuid().ToString("N")+".tmp");File.WriteAllBytes(temp,stream.ToArray());File.Move(temp,path,true);
    }
}
