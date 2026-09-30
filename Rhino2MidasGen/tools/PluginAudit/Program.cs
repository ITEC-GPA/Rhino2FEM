using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Runtime.InProcess;

internal static class Program
{
    [STAThread] static int Main(string[] args)
    {
        RhinoInside.Resolver.Initialize(@"C:\Program Files\Rhino 8\System");
        return Run(Path.GetFullPath(args.Length>0?args[0]:".."));
    }
    [MethodImpl(MethodImplOptions.NoInlining)] static int Run(string root)
    {
        Console.WriteLine("Starting RhinoCore input audit");using var rhino=new RhinoCore(["/nosplash"],WindowStyle.NoWindow);Console.WriteLine("RhinoCore ready");
        string[] projects=["Rhino2Straus","Rhino2SAP","Rhino2Midas","Rhino2MidasGen"];
        var folders=projects.Select(p=>Path.Combine(root,p,"bin")).ToArray();
        AssemblyLoadContext.Default.Resolving+=(context,name)=>{var path=folders.Select(f=>Path.Combine(f,name.Name+".dll")).FirstOrDefault(File.Exists);return path==null?null:context.LoadFromAssemblyPath(path);};
        var allIds=new HashSet<Guid>();int total=0,inputs=0;var failures=new List<string>();
        foreach(var (project,folder) in projects.Zip(folders))
        {
            var assembly=Assembly.LoadFrom(Path.Combine(folder,project+".Grasshopper.gha"));var components=new List<object>();
            using var document=new GH_Document();
            foreach(var type in assembly.GetTypes().Where(t=>!t.IsAbstract&&typeof(GH_Component).IsAssignableFrom(t)).OrderBy(t=>t.Name))
            {
                var c=(GH_Component)Activator.CreateInstance(type)!;
                if(!allIds.Add(c.ComponentGuid))throw new Exception("Duplicate GUID across plugins: "+type.FullName);
                if(c.Icon_24x24==null||c.Params.Output.Count==0)throw new Exception("Invalid icon/outputs: "+type.FullName);
                var parameters=c.Params.Input.Select((p,index)=>
                {
                    var data=p.GetType().GetProperty("PersistentData")?.GetValue(p);
                    int count=(int)(data?.GetType().GetProperty("DataCount")?.GetValue(data)??0);
                    return new{Index=index,p.Name,p.Description,Type=p.TypeName,Access=p.Access.ToString(),p.Optional,DefaultCount=count,RequiresConnection=!p.Optional&&count==0};
                }).ToArray();
                string? skipReason=type.BaseType?.Name=="NativeSectionComponent"?"Native Straus section calculation requires its API licence; inputs/defaults inspected without invoking the native engine.":null;
                bool canSolve=skipReason==null&&parameters.All(p=>!p.RequiresConnection);string[] solveErrors=[];int outputCount=0;
                if(canSolve)
                {
                    document.AddObject(c,false);
                    c.CollectData();c.ComputeData();solveErrors=c.RuntimeMessages(GH_RuntimeMessageLevel.Error).ToArray();outputCount=c.Params.Output.Sum(p=>p.VolatileDataCount);
                    if(solveErrors.Length>0||outputCount==0)failures.Add(type.FullName+": "+string.Join("; ",solveErrors)+$" (outputs {outputCount})");
                    document.RemoveObject(c,false);
                }
                components.Add(new{Class=type.FullName,c.Name,Guid=c.ComponentGuid,Inputs=parameters,DefaultSolve=canSolve,DefaultSolveErrors=solveErrors,DefaultOutputCount=outputCount,SkipReason=skipReason});inputs+=parameters.Length;total++;
            }
            Directory.CreateDirectory(Path.Combine(root,project,"docs"));
            File.WriteAllText(Path.Combine(root,project,"docs","INPUT-AUDIT.json"),JsonSerializer.Serialize(components,new JsonSerializerOptions{WriteIndented=true}));
            using(var report=JsonDocument.Parse(JsonSerializer.Serialize(components)))
            {
                var list=report.RootElement.EnumerateArray().ToArray();var ports=list.SelectMany(c=>c.GetProperty("Inputs").EnumerateArray()).ToArray();
                File.WriteAllText(Path.Combine(root,project,"docs","INPUT-AUDIT.md"),$"# Controllo ingressi — {DateTime.Now:yyyy-MM-dd}\n\n{list.Length} componenti e {ports.Length} ingressi ispezionati in Rhino/Grasshopper reali, con tutti e quattro i plugin caricati insieme e nessuna collisione GUID.\n\n"+
                    $"- Ingressi con dati predefiniti: {ports.Count(p=>p.GetProperty("DefaultCount").GetInt32()>0)}.\n- Ingressi marcati opzionali: {ports.Count(p=>p.GetProperty("Optional").GetBoolean())}; possono avere anche un default.\n- Ingressi che richiedono dati del modello o parametri espliciti: {ports.Count(p=>p.GetProperty("RequiresConnection").GetBoolean())}.\n- Componenti eseguiti con soli default/opzioni scollegate: {list.Count(c=>c.GetProperty("DefaultSolve").GetBoolean())}.\n- Sezioni native Straus escluse dal calcolo per dipendenza dalla licenza API: {list.Count(c=>c.GetProperty("SkipReason").ValueKind!=JsonValueKind.Null)}; tutti i loro ingressi sono stati ispezionati.\n\n"+
                    "[Inventario completo per componente e ingresso](INPUT-AUDIT.json). I dati indispensabili — geometria, Model, proprietà, nomi/riferimenti e argomenti obbligatori delle API — richiedono ancora un collegamento. I default geometrici sono esempi modificabili nelle unità del Model.\n\n"+
                    "Ripetere dalla cartella Rhino2MidasGen: `dotnet run --project tools/PluginAudit -c Release -- ..`, dopo build.ps1 dei quattro progetti (assembly in bin). Il controllo automatico dei default non avvia i solver strutturali.\n");
            }
            Regression(assembly,project,document);
            Console.WriteLine($"PASS {project}: {components.Count} components, valid icons/outputs and no GUID collisions. Input inventory saved.");
        }
        if(failures.Count>0)throw new Exception(string.Join("\n",failures));
        Console.WriteLine($"AUDIT COMPLETE: {total} components, {inputs} inputs across all four plugins.");return 0;
    }
    static void Regression(Assembly assembly,string project,GH_Document doc)
    {
        GH_Component Create(string name){var c=(GH_Component)Activator.CreateInstance(assembly.GetTypes().Single(t=>t.Name==name))!;doc.AddObject(c,false);return c;}
        void Solve(GH_Component c){c.CollectData();c.ComputeData();var errors=c.RuntimeMessages(GH_RuntimeMessageLevel.Error);if(errors.Count>0)throw new Exception(c.Name+": "+string.Join("; ",errors));}
        void Wire(GH_Component source,GH_Component target,int input)=>target.Params.Input[input].AddSource(source.Params.Output[0]);
        void Expect(GH_Component c,int output,int count){Solve(c);if(c.Params.Output[output].VolatileDataCount!=count)throw new Exception(c.Name+": wrong output count");}
        if(project=="Rhino2Straus")
        {
            var group=Create("ElementGroupComponent");Solve(group);var material=Create("MaterialComponent");Solve(material);var section=Create("FrameSectionRectangularComponent");Solve(section);
            var property=Create("FramePropertyComponent");Wire(section,property,1);Wire(material,property,2);Solve(property);
            var beam=Create("FrameElementComponent");((Param_Curve)beam.Params.Input[0]).PersistentData.Append(new GH_Curve(new LineCurve(Point3d.Origin,new Point3d(3,0,0))));Wire(property,beam,1);Wire(group,beam,3);Expect(beam,0,1);
            foreach(bool byGroup in new[]{false,true}){var filter=Create("FrameFilterComponent");Wire(beam,filter,0);if(byGroup)Wire(group,filter,4);Expect(filter,0,1);}
            var node=Create("NodeElementComponent");((Param_Point)node.Params.Input[0]).PersistentData.Append(new GH_Point(Point3d.Origin));Wire(group,node,1);Expect(node,0,1);
            foreach(bool byGroup in new[]{false,true}){var filter=Create("NodeFilterComponent");Wire(node,filter,0);if(byGroup)Wire(group,filter,2);Expect(filter,0,1);}
            var thickness=Create("AreaThicknessComponent");Solve(thickness);var areaProperty=Create("AreaPropertyComponent");Wire(thickness,areaProperty,1);Wire(material,areaProperty,2);Solve(areaProperty);
            var area=Create("AreaElementComponent");var mesh=new Mesh();mesh.Vertices.Add(0,0,0);mesh.Vertices.Add(1,0,0);mesh.Vertices.Add(0,1,0);mesh.Faces.AddFace(0,1,2);((Param_Mesh)area.Params.Input[0]).PersistentData.Append(new GH_Mesh(mesh));Wire(areaProperty,area,1);Wire(group,area,3);Expect(area,0,1);
            foreach(int criterion in new[]{0,1,2}){var filter=Create("AreaFilterComponent");Wire(area,filter,0);if(criterion==1)((Param_Point)filter.Params.Input[1]).PersistentData.Append(new GH_Point(Point3d.Origin));if(criterion==2)Wire(group,filter,3);Expect(filter,0,1);}
            var build=Create("BuildModelComponent");Wire(beam,build,2);Expect(build,0,1);
            var loadCase=Create("LoadCaseComponent");Solve(loadCase);var gravity=Create("SelfWeightComponent");Wire(loadCase,gravity,0);Expect(gravity,0,1);
            Console.WriteLine("PASS Straus optional groups/position filters and default units through real GH wires.");
        }
        if(project is "Rhino2Midas" or "Rhino2MidasGen")
        {
            var example=Create("ExampleModelComponent");Solve(example);
            foreach(string name in new[]{"MoveModelComponent","RotateModelComponent"}){var c=Create(name);Wire(example,c,0);Expect(c,0,1);}
            var requestType=assembly.GetTypes().First(t=>t.Name.StartsWith("Request_"));var request=(GH_Component)Activator.CreateInstance(requestType)!;doc.AddObject(request,false);Solve(request);
            var reader=Create("ReadNativeResultTableComponent");Wire(request,reader,0);Solve(reader);
            Console.WriteLine("PASS "+project+" default move/rotation and unconnected result units; Run remains false.");
        }
        if(project=="Rhino2SAP")
        {
            var schema=Assembly.Load("Rhino2SAP.Core").GetType("Rhino2SAP.Core.ApiSchema")!;
            foreach(var type in assembly.GetTypes().Where(t=>t.BaseType?.Name=="ApiCommandComponent"))
            {
                var c=(GH_Component)Activator.CreateInstance(type)!;string key=(string)type.GetProperty("MethodKey",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(c)!;
                dynamic method=schema.GetMethod("Get")!.Invoke(null,[key])!;int i=1;
                foreach(dynamic parameter in method.Parameters){if(parameter.Optional&&!c.Params.Input[i].Optional)throw new Exception(c.Name+": SDK optional input marked required");if(parameter.Optional&&parameter.Type=="System.String"&&parameter.Default.ValueKind==JsonValueKind.String&&((Param_GenericObject)c.Params.Input[i]).PersistentData.DataCount!=1)throw new Exception(c.Name+": SDK string default missing");i++;}
            }
            Console.WriteLine("PASS SAP optional flags and string defaults match the installed SDK schema for all generated commands.");
        }
    }
}
