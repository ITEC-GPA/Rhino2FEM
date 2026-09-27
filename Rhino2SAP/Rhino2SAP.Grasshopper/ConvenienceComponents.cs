using Grasshopper.Kernel;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public sealed class MaterialComponent:SafeComponent
{
    public MaterialComponent():base("SAP Isotropic Material","Material","Define material type, elasticity and weight density in Model force/length/temperature units.","01-Materials"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Name","N","Material name.",GH_ParamAccess.item);p.AddIntegerParameter("Type","T","1 Steel, 2 Concrete, 3 NoDesign, 4 Aluminum, 5 ColdFormed, 6 Rebar, 7 Tendon.",GH_ParamAccess.item,1);p.AddNumberParameter("E","E","Elastic modulus [F/L²].",GH_ParamAccess.item,210e6);p.AddNumberParameter("Poisson","ν","Poisson ratio.",GH_ParamAccess.item,.3);p.AddNumberParameter("Alpha","α","Thermal coefficient [1/T].",GH_ParamAccess.item,1.2e-5);p.AddNumberParameter("Weight density","γ","Weight per volume [F/L³], not mass density.",GH_ParamAccess.item,78.5);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Material","M","Material definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,StandardDefinitions.Material(Item<string>(da,0),Item<int>(da,1),Item<double>(da,2),Item<double>(da,3),Item<double>(da,4),Item<double>(da,5)));
}
public sealed class LoadPatternComponent:SafeComponent
{
    public LoadPatternComponent():base("SAP Load Pattern","Pattern","Define a load pattern and matching linear static analysis case.","06-Cases"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Name","N","Load pattern name.",GH_ParamAccess.item);p.AddIntegerParameter("Type","T","1 Dead, 2 SuperDead, 3 Live, 4 ReduceLive, 5 Quake, 6 Wind, 7 Snow, 8 Other.",GH_ParamAccess.item,8);p.AddNumberParameter("Self weight","W","Gravity self-weight multiplier, usually 1 only in DEAD.",GH_ParamAccess.item,0);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Pattern","P","Pattern and linear case.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,StandardDefinitions.LoadPattern(Item<string>(da,0),Item<int>(da,1),Item<double>(da,2)));
}
public abstract class StaticCaseComponent:SafeComponent
{
    protected abstract string Solver{get;}
    protected StaticCaseComponent(string name):base(name,name,"Create a SAP analysis case with load pattern factors. Configure further solver options through native case components.","06-Cases"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Name","N","Case name.",GH_ParamAccess.item);p.AddTextParameter("Patterns","P","Load pattern names.",GH_ParamAccess.list);p.AddNumberParameter("Factors","F","One factor per pattern.",GH_ParamAccess.list);p.AddGenericParameter("Definitions","D","Load pattern dependencies.",GH_ParamAccess.list);p[3].Optional=true;}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Case","C","Case definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var p=new List<string>();var f=new List<double>();var d=new List<object>();da.GetDataList(1,p);da.GetDataList(2,f);da.GetDataList(3,d);Output(da,0,Fragment.Combine(d.Select(FragmentOf).Append(StandardDefinitions.StaticCase(Item<string>(da,0),p,f,Solver))));}
}
public sealed class LinearCaseComponent:StaticCaseComponent{protected override string Solver=>"StaticLinear";public LinearCaseComponent():base("SAP Linear Static Case"){} }
public sealed class NonlinearCaseComponent:StaticCaseComponent{protected override string Solver=>"StaticNonlinear";public NonlinearCaseComponent():base("SAP Nonlinear Static Case"){} }
public sealed class BucklingCaseComponent:StaticCaseComponent{protected override string Solver=>"Buckling";public BucklingCaseComponent():base("SAP Buckling Case"){} }
public sealed class CombinationComponent:SafeComponent
{
    public CombinationComponent():base("SAP Load Combination","Combination","Create linear add/envelope/absolute/SRSS/range combination from cases or nested combinations.","06-Cases"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Name","N","Combination name.",GH_ParamAccess.item);p.AddIntegerParameter("Type","T","0 LinearAdd, 1 Envelope, 2 AbsoluteAdd, 3 SRSS, 4 RangeAdd.",GH_ParamAccess.item,0);p.AddTextParameter("Cases","C","Case/combination names.",GH_ParamAccess.list);p.AddNumberParameter("Factors","F","One factor per entry.",GH_ParamAccess.list);p.AddBooleanParameter("Is combination","Co","Optional flags per entry (default all cases).",GH_ParamAccess.list);p[4].Optional=true;}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Combination","C","Combination definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var names=new List<string>();var factors=new List<double>();var flags=new List<bool>();da.GetDataList(2,names);da.GetDataList(3,factors);da.GetDataList(4,flags);Output(da,0,StandardDefinitions.Combination(Item<string>(da,0),Item<int>(da,1),names,factors,flags.Count==0?null:flags));}
}
public sealed class ExampleModelComponent:SafeComponent
{
    public ExampleModelComponent():base("SAP Axial Verification Example","Example","3 m cantilever, 0.2 x 0.1 m section, E=210e6 kN/m², 1 kN axial tip force. Expected Ux = 3/(210e6*0.02) m.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Model","M","Ready to export/run, kN_m_C.",GH_ParamAccess.item);p.AddNumberParameter("Expected Ux","U","Analytical axial tip displacement [m].",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){Output(da,0,StandardDefinitions.AxialExample());da.SetData(1,3/(210e6*.02));}
}
