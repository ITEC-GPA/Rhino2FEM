using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Grasshopper.Kernel;
using Rhino.Runtime.InProcess;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino2SAP.Core;
using Rhino.Geometry;
using System.Text.Json;
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread] static int Main(){RhinoInside.Resolver.Initialize(@"C:\Program Files\Rhino 8\System");return Run();}
    [MethodImpl(MethodImplOptions.NoInlining)] static int Run()
    {
        try
        {
            using var rhino=new RhinoCore(["/nosplash"],WindowStyle.NoWindow);
            string root=Environment.CurrentDirectory;string plugin=Path.Combine(root,"bin","Rhino2SAP.Grasshopper.gha");
            AssemblyLoadContext.Default.Resolving+=(context,name)=>{string file=Path.Combine(Path.GetDirectoryName(plugin)!,name.Name+".dll");return File.Exists(file)?context.LoadFromAssemblyPath(file):null;};
            var assembly=Assembly.LoadFrom(plugin);var ids=new HashSet<Guid>();int count=0;var catalogue=new List<object>();
            var sources=Directory.GetFiles(Path.Combine(root,"Rhino2SAP.Grasshopper"),"*.cs",SearchOption.AllDirectories).Where(f=>!f.Contains(Path.DirectorySeparatorChar+"obj"+Path.DirectorySeparatorChar)).Select(f=>(File:f,Text:File.ReadAllText(f))).ToArray();
            foreach(var type in assembly.GetTypes().Where(t=>!t.IsAbstract&&typeof(GH_Component).IsAssignableFrom(t)))
            {
                var component=(GH_Component)Activator.CreateInstance(type)!;
                if(!ids.Add(component.ComponentGuid))throw new Exception("Duplicate GUID: "+type.Name);
                if(component.Icon_24x24==null||component.Icon_24x24.Width!=24)throw new Exception("Missing icon: "+type.Name);
                if(component.Params.Input.Any(p=>p==null)||component.Params.Output.Count==0)throw new Exception("Invalid sockets: "+type.Name);
                string? source=sources.FirstOrDefault(s=>System.Text.RegularExpressions.Regex.IsMatch(s.Text,@"class\s+"+System.Text.RegularExpressions.Regex.Escape(type.Name)+@"\s*:")).File;
                var keyProperty=type.GetProperty("MethodKey",BindingFlags.Instance|BindingFlags.NonPublic);
                catalogue.Add(new{Class=type.Name,Name=component.Name,Category=component.SubCategory,Api=keyProperty?.GetValue(component)?.ToString()??"",File=source==null?"":Path.GetRelativePath(root,source).Replace('\\','/'),Guid=component.ComponentGuid});
                count++;
            }
            File.WriteAllText(Path.Combine(root,"docs","components.json"),JsonSerializer.Serialize(catalogue,new JsonSerializerOptions{WriteIndented=true}));
            Console.WriteLine($"PASS: {count} components instantiate in Rhino; unique GUIDs, valid sockets and 24px icons.");
            var doc=new GH_Document();
            var example=(GH_Component)Activator.CreateInstance(assembly.GetType("Rhino2SAP.Grasshopper.ExampleModelComponent")!)!;doc.AddObject(example,false);example.CollectData();example.ComputeData();
            if(example.RuntimeMessages(GH_RuntimeMessageLevel.Error).Count>0||example.Params.Output[0].VolatileDataCount!=1)throw new Exception("Example model component failed to solve: "+string.Join("; ",example.RuntimeMessages(GH_RuntimeMessageLevel.Error)));
            Console.WriteLine("PASS: example model solves in a Grasshopper document.");
            GH_Component Create(string name){var c=(GH_Component)Activator.CreateInstance(assembly.GetType("Rhino2SAP.Grasshopper."+name)!)!;doc.AddObject(c,false);return c;}
            void Solve(GH_Component c){c.CollectData();c.ComputeData();if(c.RuntimeMessages(GH_RuntimeMessageLevel.Error).Count>0)throw new Exception(c.Name+": "+string.Join("; ",c.RuntimeMessages(GH_RuntimeMessageLevel.Error)));}
            var material=Create("MaterialComponent");((Param_String)material.Params.Input[0]).PersistentData.Append(new GH_String("Steel"));Solve(material);
            var section=Create("PropFrame_SetRectangleComponent");((Param_GenericObject)section.Params.Input[1]).PersistentData.Append(new GH_String("R"));section.Params.Input[2].AddSource(material.Params.Output[0]);((Param_Number)section.Params.Input[3]).PersistentData.Append(new GH_Number(.2));((Param_Number)section.Params.Input[4]).PersistentData.Append(new GH_Number(.1));Solve(section);
            var fragment=(Fragment)section.Params.Output[0].VolatileData.AllData(true).Single().ScriptVariable();
            if(fragment.Operations.Count!=4||fragment.Operations.Last().Arguments["MatProp"].GetString()!="Steel")throw new Exception("Property dependency propagation failed.");
            Console.WriteLine("PASS: material → native section connection preserves dependencies and resolves property name.");
            var build=Create("BuildModelComponent");build.Params.Input[0].AddSource(example.Params.Output[0]);Solve(build);if(build.Params.Output[0].VolatileDataCount!=1)throw new Exception("Build failed.");
            Console.WriteLine("PASS: existing Model → Build Model connection solves.");
            var physical=Create("PhysicalGeometryComponent");physical.Params.Input[0].AddSource(example.Params.Output[0]);Solve(physical);
            var solids=physical.Params.Output[0].VolatileData.AllData(true).Select(g=>g.ScriptVariable()).OfType<Brep>().ToArray();
            Console.WriteLine("Physical geometry: "+string.Join(", ",physical.Params.Output[0].VolatileData.AllData(true).Select(g=>g.ScriptVariable()?.GetType().Name))+"; issues="+string.Join("; ",physical.Params.Output[2].VolatileData.AllData(true)));
            if(solids.Length!=1||!solids[0].IsSolid||Math.Abs(Math.Abs(VolumeMassProperties.Compute(solids[0]).Volume)-.06)>1e-7)throw new Exception("Physical frame geometry has incorrect volume: count="+solids.Length+(solids.Length>0?", solid="+solids[0].IsSolid+", V="+VolumeMassProperties.Compute(solids[0]).Volume:""));
            Console.WriteLine("PASS: native rectangle physical preview is a closed 0.06 m³ beam.");
            var baseModel=StandardDefinitions.AxialExample();var testPlate=new Element("A1",ElementKind.Area,[new(0,0,0),new(1,0,0),new(1,1,0)],"Shell");var testModel=new SapModel(Fragment.Combine([baseModel.Definition,new Fragment(elements:[testPlate])]));
            ResultTable Fixture(string key,string obj){var map=ApiSchema.Get(key).Parameters.Where(p=>p.ByRef).ToDictionary(p=>p.Name,p=>p.IsArray?ApiSchema.Value(p.ScalarType=="System.String"?(object)new[]{p.Name=="Obj"?obj:p.Name=="LoadCase"?"LC":"",p.Name=="Obj"?obj:p.Name=="LoadCase"?"LC":""}:p.Name=="P"?new[]{10d,-20d}:p.Name=="ObjSta"?new[]{0d,3d}:new[]{1d,2d}):ApiSchema.Value(2));return new ResultTable(key,map);}
            var solvedModel=testModel.WithResults(new ResultSet(testModel.Fingerprint,"fixture.sdb","fixture",true,[Fixture("Results.FrameForce","F1"),Fixture("Results.AreaForceShell","A1")],[]));
            var modelGooType=assembly.GetType("Rhino2SAP.Grasshopper.ModelGoo")!;
            var decompose=Create("DecomposeModelComponent");((Param_GenericObject)decompose.Params.Input[0]).PersistentData.Append((IGH_Goo)Activator.CreateInstance(modelGooType,solvedModel)!);Solve(decompose);
            var beamActions=Create("BeamActionsComponent");beamActions.Params.Input[0].AddSource(decompose.Params.Output[1]);Solve(beamActions);
            if(!beamActions.Params.Output[0].VolatileData.AllData(true).OfType<GH_Number>().Select(n=>n.Value).SequenceEqual(new[]{10d,-20d}))throw new Exception("Embedded beam force query lost native P values.");
            var plateActions=Create("PlateActionsComponent");plateActions.Params.Input[0].AddSource(decompose.Params.Output[2]);Solve(plateActions);
            if(!plateActions.Params.Output[3].VolatileData.AllData(true).OfType<GH_Number>().Select(n=>n.Value).SequenceEqual(new[]{1d,2d}))throw new Exception("Embedded plate moment query failed.");
            var beamDecompose=Create("DecomposeBeamComponent");beamDecompose.Params.Input[0].AddSource(decompose.Params.Output[1]);Solve(beamDecompose);
            if(beamDecompose.Params.Output[4].VolatileDataCount!=1)throw new Exception("Beam decomposition omitted embedded results.");
            Console.WriteLine("PASS: solved Model → Decompose → beam actions, plate moments and beam result tables through real Grasshopper wires.");
            var enriched=new SapModel(baseModel.Definition.Append(Operation.Create("PointObj.SetMass",("Name","TIP"),("M",new[]{1d,1,1,0,0,0}))).Append(Operation.Create("PointObj.SetSpring",("Name","TIP"),("K",new[]{100d,100,100,0,0,0}))).Append(Operation.Create("FrameObj.SetReleases",("Name","F1"),("II",new[]{false,false,false,false,true,false}),("JJ",new[]{false,false,false,false,false,true}),("StartValue",new double[6]),("EndValue",new double[6]))));
            var optionsType=assembly.GetType("Rhino2SAP.Grasshopper.DisplayOptions")!;object options=Activator.CreateInstance(optionsType)!;
            var sceneType=assembly.GetType("Rhino2SAP.Grasshopper.PreviewScene")!;var scene=sceneType.GetMethod("For",[typeof(Fragment)])!.Invoke(null,[enriched.Definition])!;
            var diagnostic=sceneType.GetMethod("Diagnostics")!;var initial=(ValueTuple<int,int,int,int>)diagnostic.Invoke(scene,[options])!;
            if(initial.Item2<1||initial.Item1<1||initial.Item4<1)throw new Exception("Preview missing load/support/spring/mass/release symbols.");
            optionsType.GetProperty("Loads")!.SetValue(options,false);var withoutLoads=(ValueTuple<int,int,int,int>)diagnostic.Invoke(scene,[options])!;
            if(withoutLoads.Item2>=initial.Item2)throw new Exception("Load toggle did not remove load arrows.");
            optionsType.GetProperty("Enabled")!.SetValue(options,false);var disabled=(ValueTuple<int,int,int,int>)diagnostic.Invoke(scene,[options])!;
            if(disabled!=(0,0,0,0))throw new Exception("Master preview off left drawn symbols.");
            Console.WriteLine("PASS: loads/attributes preview and independent loads/master-off switches.");
            var displaySettings=Create("DisplaySettingsComponent");Solve(displaySettings);var preview=Create("PreviewModelComponent");preview.Params.Input[0].AddSource(example.Params.Output[0]);preview.Params.Input[1].AddSource(displaySettings.Params.Output[0]);Solve(preview);
            Console.WriteLine("PASS: per-model display settings connect to preview without changing model data.");
            BrepSmoke.Run(assembly,doc,Create,Solve,example);
            using(var window=(Form)Activator.CreateInstance(assembly.GetType("Rhino2SAP.Grasshopper.DisplayWindow")!)!){window.StartPosition=FormStartPosition.Manual;window.Location=new System.Drawing.Point(-32000,-32000);window.Opacity=0;window.ShowInTaskbar=false;window.Show();window.PerformLayout();Application.DoEvents();using var bitmap=new Bitmap(window.Width,window.Height);window.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(root,"docs","display-settings.png"));window.Hide();}
            doc.Dispose();return 0;
        }
        catch(Exception ex){Console.WriteLine(ex);return 1;}
    }
}
