using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2SAP.Core;

internal static class BrepSmoke
{
    public static void Run(Assembly assembly,GH_Document gh,Func<string,GH_Component> create,Action<GH_Component> solve,GH_Component example)
    {
        var modelType=assembly.GetType("Rhino2SAP.Grasshopper.ModelGoo")!;
        var fragmentType=assembly.GetType("Rhino2SAP.Grasshopper.FragmentGoo")!;
        IGH_Goo Goo(SapModel model)=>(IGH_Goo)Activator.CreateInstance(modelType,model)!;
        void ModelInput(GH_Component component,SapModel model)=>((Param_GenericObject)component.Params.Input[0]).PersistentData.Append(Goo(model));
        void CheckSolid(Brep brep,double volume)
        {
            using var mass=VolumeMassProperties.Compute(brep);
            if(!brep.IsValid||!brep.IsSolid||brep.SolidOrientation!=BrepSolidOrientation.Outward||mass==null||Math.Abs(mass.Volume-volume)>1e-7)
                throw new Exception($"Invalid solid: expected V={volume}, actual V={mass?.Volume}, valid={brep.IsValid}, closed={brep.IsSolid}, orientation={brep.SolidOrientation}.");
        }
        GH_Component Preview(SapModel model)
        {
            var preview=create("PreviewModelComponent");ModelInput(preview,model);solve(preview);return preview;
        }
        Brep[] Breps(GH_Component component)=>component.Params.Output[2].VolatileData.AllData(true).OfType<GH_Brep>().Select(b=>b.Value).ToArray();

        var axial=StandardDefinitions.AxialExample();
        var shell=Operation.Create("PropArea.SetShell_1",("Name","Shell"),("ShellType",1),("IncludeDrillingDOF",true),("MatProp","S355"),("MatAng",0d),("Thickness",.2),("Bending",.2));
        var plate=new Element("A1",ElementKind.Area,[new(0,0,0),new(1,0,0),new(1,1,0)],"Shell");
        var brick=new Element("S1",ElementKind.Solid,[new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0),new(0,0,1),new(1,0,1),new(1,1,1),new(0,1,1)],"Solid");
        var model=new SapModel(Fragment.Combine([axial.Definition,new Fragment([shell],[plate,brick])]));
        var preview=Preview(model);var breps=Breps(preview);
        string[] names=preview.Params.Output[3].VolatileData.AllData(true).OfType<GH_String>().Select(s=>s.Value).ToArray();
        if(!names.SequenceEqual(new[]{"F1","A1","S1"})||breps.Length!=3)throw new Exception("Preview Breps/names lost element correspondence.");
        CheckSolid(breps[0],.06);CheckSolid(breps[1],.1);CheckSolid(breps[2],1);
        var reverse=Preview(new SapModel(new Fragment([shell],[plate with{Points=plate.Points.Reverse().ToArray()}])));
        CheckSolid(Breps(reverse).Single(),.1);

        var profiles=new (Operation Section,double Volume)[]
        {
            (Operation.Create("PropFrame.SetCircle",("Name","P"),("MatProp","S355"),("T3",.2)),Math.PI*.1*.1*3),
            (Operation.Create("PropFrame.SetPipe",("Name","P"),("MatProp","S355"),("T3",.2),("TW",.01)),Math.PI*(.1*.1-.09*.09)*3),
            (Operation.Create("PropFrame.SetTube",("Name","P"),("MatProp","S355"),("T3",.2),("T2",.1),("Tf",.01),("Tw",.01)),(.2*.1-.18*.08)*3)
        };
        foreach(var profile in profiles)
        {
            var member=new Element("Profile",ElementKind.Frame,[new(0,0,0),new(3,0,0)],"P");
            var output=Preview(new SapModel(new Fragment([profile.Section],[member])));
            var solid=Breps(output);
            if(solid.Length!=1)throw new Exception(profile.Section.Key+": "+string.Join("; ",output.Params.Output[1].VolatileData.AllData(true)));
            CheckSolid(solid[0],profile.Volume);
        }
        Console.WriteLine("PASS: Brep preview volumes for rectangle, circle, hollow pipe/tube, shells in both orientations and solid elements.");

        GH_Brep cast=null!;var single=Goo(axial);
        if(!single.CastTo(out cast))throw new Exception("Single-element Model → Brep cast failed.");CheckSolid(cast.Value,.06);
        cast.Value.Translate(Vector3d.XAxis*100);GH_Brep again=null!;
        if(!single.CastTo(out again)||again.Value.GetBoundingBox(true).Min.X>10)throw new Exception("Brep cast mutated the cached model geometry.");
        GH_Brep ambiguous=null!;if(Goo(model).CastTo(out ambiguous))throw new Exception("Multi-element Model cast silently discarded elements.");
        var decomposed=create("DecomposeModelComponent");decomposed.Params.Input[0].AddSource(example.Params.Output[0]);solve(decomposed);
        GH_Brep decomposedBrep=null!;
        if(!decomposed.Params.Output[1].VolatileData.AllData(true).Single().CastTo(out decomposedBrep))throw new Exception("Decomposed beam lost property dependencies needed for Brep cast.");
        CheckSolid(decomposedBrep.Value,.06);
        var sectionOnly=(IGH_Goo)Activator.CreateInstance(fragmentType,new Fragment(axial.Definition.Operations.Where(o=>o.Key=="PropFrame.SetRectangle")))!;
        GH_Brep sectionBrep=null!;if(!sectionOnly.CastTo(out sectionBrep))throw new Exception("Section sample cast failed.");CheckSolid(sectionBrep.Value,.002);
        var brepParameter=new Param_Brep();gh.AddObject(brepParameter,false);brepParameter.AddSource(preview.Params.Output[2]);brepParameter.CollectData();brepParameter.ComputeData();
        if(brepParameter.VolatileDataCount!=3)throw new Exception("Preview → native GH Brep parameter failed.");
        Console.WriteLine("PASS: Model/element/section Brep casts, independent Brep copies, decomposed beam dependencies, native Brep parameter and rejection of ambiguous multi-element casts.");

        var missing=new Element("MissingSection",ElementKind.Frame,[new(0,0,0),new(0,2,0)],"Unknown");
        var partial=Preview(new SapModel(Fragment.Combine([axial.Definition,new Fragment(elements:[missing])])));
        if(Breps(partial).Length!=1||!partial.Params.Output[1].VolatileData.AllData(true).Any(s=>s.ToString().Contains("MissingSection")))throw new Exception("Unsupported physical element was not explicitly skipped.");
        var options=Activator.CreateInstance(assembly.GetType("Rhino2SAP.Grasshopper.DisplayOptions")!)!;options.GetType().GetProperty("Enabled")!.SetValue(options,false);
        var off=create("PreviewModelComponent");ModelInput(off,model);((Param_GenericObject)off.Params.Input[1]).PersistentData.Append(new GH_ObjectWrapper(options));solve(off);
        if(Breps(off).Length!=3)throw new Exception("Turning off preview destroyed the Brep output.");
        Console.WriteLine("PASS: unsupported geometry reports Issues; preview visibility does not affect solid output.");

        using var rhinoDoc=Rhino.RhinoDoc.Create(null)??throw new Exception("Cannot create isolated Rhino test document.");
        var baked=new List<Guid>();((IGH_BakeAwareObject)preview).BakeGeometry(rhinoDoc,baked);
        if(baked.Count!=3||baked.Any(id=>rhinoDoc.Objects.FindId(id)?.Geometry is not Brep b||!b.IsSolid))throw new Exception("Native preview Bake did not produce closed Breps.");
        var bake=create("BakeGeometryComponent");ModelInput(bake,model);
        var trigger=(Param_Boolean)bake.Params.Input[2];trigger.PersistentData.Clear();trigger.PersistentData.Append(new GH_Boolean(true));solve(bake);
        var ids=bake.Params.Output[0].VolatileData.AllData(true).Select(s=>Guid.Parse(s.ToString())).ToArray();
        if(ids.Length!=3||!ids.Select(id=>rhinoDoc.Objects.FindId(id)!.Attributes.Name).SequenceEqual(names)||ids.Any(id=>rhinoDoc.Objects.FindId(id)?.Geometry is not Brep b||!b.IsSolid))throw new Exception("Solid-only bake returned analytical objects or mismatched SAP names.");
        int count=rhinoDoc.Objects.Count; bake.ExpireSolution(false);solve(bake);
        if(rhinoDoc.Objects.Count!=count)throw new Exception("Bake repeated without a new rising edge.");
        Console.WriteLine("PASS: native preview Bake and Bake SAP Geometry create actual closed Rhino Breps; SAP names retained; no duplicate bake while trigger remains true.");
    }
}
