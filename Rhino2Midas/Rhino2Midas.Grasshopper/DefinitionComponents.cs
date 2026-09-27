using Grasshopper.Kernel;
using Rhino2Midas.Core;

namespace Rhino2Midas.Grasshopper;

public sealed class MaterialComponent:SafeComponent
{
    public MaterialComponent():base("Midas Isotropic Material","Material","Elastic modulus F/L², weight density F/L³; values use Model units.","01-Materials"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Material ID.",GH_ParamAccess.item,1);p.AddTextParameter("Name","N","Name.",GH_ParamAccess.item,"Steel");p.AddNumberParameter("E","E","Elastic modulus F/L².",GH_ParamAccess.item,210000000);p.AddNumberParameter("Poisson","v","Poisson ratio.",GH_ParamAccess.item,.3);p.AddNumberParameter("Weight density","W","Weight per volume F/L³.",GH_ParamAccess.item,78.5);p.AddNumberParameter("Thermal","a","Thermal expansion per Model temperature degree.",GH_ParamAccess.item,1.2e-5);p.AddTextParameter("Type","T","STEEL, CONC, USER.",GH_ParamAccess.item,"STEEL");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Material","M","Midas material definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.Material(Item<int>(da,0),Item<string>(da,1),Item<double>(da,2),Item<double>(da,3),Item<double>(da,4),Item<double>(da,5),Item<string>(da,6)));
}
public sealed class DatabaseMaterialComponent:SafeComponent
{
    public DatabaseMaterialComponent():base("Midas Database Material","DB Material","Use a material in the Civil material library.","01-Materials"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Material ID.",GH_ParamAccess.item,1);p.AddTextParameter("Name","N","Name.",GH_ParamAccess.item,"S355");p.AddTextParameter("Type","T","STEEL or CONC.",GH_ParamAccess.item,"STEEL");p.AddTextParameter("Standard","S","Exact Civil standard.",GH_ParamAccess.item,"EN05(S)");p.AddTextParameter("Grade","G","Exact database grade.",GH_ParamAccess.item,"S355");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Material","M","Database material.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.DatabaseMaterial(Item<int>(da,0),Item<string>(da,1),Item<string>(da,2),Item<string>(da,3),Item<string>(da,4)));
}
public abstract class SectionComponent:SafeComponent
{
    protected abstract string Shape{get;}
    protected abstract string[] Dimensions{get;}
    protected SectionComponent(string name):base("Midas "+name+" Section",name,"Native Civil section. Dimensions use Model length units. y is horizontal and z vertical in the section plane.","02-Properties"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Section ID.",GH_ParamAccess.item,1);p.AddTextParameter("Name","N","Section name.",GH_ParamAccess.item,Shape);p.AddTextParameter("Insertion","I","LT/CT/RT/LC/CC/RC/LB/CB/RB.",GH_ParamAccess.item,"CC");foreach(var d in Dimensions)p.AddNumberParameter(d,d,d+" in model length units. Initial dimensions are an editable example.",GH_ParamAccess.item,d switch {"Height"=>.3,"Diameter" or "Width" or "Top width" or "Bottom width"=>.2,"Web thickness" or "Wall"=>.01,_=>.02});}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Section","S","Section definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.Section(Item<int>(da,0),Item<string>(da,1),Shape,Dimensions.Select((_,i)=>Item<double>(da,i+3)).ToArray(),Item<string>(da,2)));
}
public sealed class RectangleSectionComponent:SectionComponent{protected override string Shape=>"SB";protected override string[] Dimensions=>["Height","Width"];public RectangleSectionComponent():base("Rectangle"){} }
public sealed class CircleSectionComponent:SectionComponent{protected override string Shape=>"SR";protected override string[] Dimensions=>["Diameter"];public CircleSectionComponent():base("Circle"){} }
public sealed class PipeSectionComponent:SectionComponent{protected override string Shape=>"P";protected override string[] Dimensions=>["Diameter","Wall"];public PipeSectionComponent():base("Pipe"){} }
public sealed class BoxSectionComponent:SectionComponent{protected override string Shape=>"B";protected override string[] Dimensions=>["Height","Width","Web thickness","Flange thickness"];public BoxSectionComponent():base("Box"){} }
public sealed class ISectionComponent:SectionComponent{protected override string Shape=>"H";protected override string[] Dimensions=>["Height","Top width","Web thickness","Top thickness","Bottom width","Bottom thickness"];public ISectionComponent():base("I"){} }
public sealed class TSectionComponent:SectionComponent{protected override string Shape=>"T";protected override string[] Dimensions=>["Height","Width","Web thickness","Flange thickness"];public TSectionComponent():base("Tee"){} }
public sealed class ChannelSectionComponent:SectionComponent{protected override string Shape=>"C";protected override string[] Dimensions=>["Height","Width","Web thickness","Flange thickness"];public ChannelSectionComponent():base("Channel"){} }
public sealed class AngleSectionComponent:SectionComponent{protected override string Shape=>"L";protected override string[] Dimensions=>["Height","Width","Web thickness","Flange thickness"];public AngleSectionComponent():base("Angle"){} }
public sealed class DatabaseSectionComponent:SafeComponent
{
    public DatabaseSectionComponent():base("Midas Database Section","DB Section","Native Civil section library.","02-Properties"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Section ID.",GH_ParamAccess.item,1);p.AddTextParameter("Name","N","Property name.",GH_ParamAccess.item);p.AddTextParameter("Shape","S","Native shape, e.g. H.",GH_ParamAccess.item,"H");p.AddTextParameter("Database","D","Native database name.",GH_ParamAccess.item,"DIN");p.AddTextParameter("Section","P","Native profile name.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Section","S","Native definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.DatabaseSection(Item<int>(da,0),Item<string>(da,1),Item<string>(da,2),Item<string>(da,3),Item<string>(da,4)));
}
public sealed class ThicknessComponent:SafeComponent
{
    public ThicknessComponent():base("Midas Plate Thickness","Thickness","Membrane/bending thickness and physical offset.","02-Properties"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Property ID.",GH_ParamAccess.item,1);p.AddNumberParameter("Thickness","T","Membrane thickness.",GH_ParamAccess.item,.2);p.AddNumberParameter("Bending thickness","B","Zero = same thickness.",GH_ParamAccess.item,0);p.AddNumberParameter("Offset","O","Value along plate normal.",GH_ParamAccess.item,0);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Thickness","T","Thickness definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.Thickness(Item<int>(da,0),Item<double>(da,1),Item<double>(da,2),Item<double>(da,3)));
}
public sealed class LoadCaseComponent:SafeComponent
{
    public LoadCaseComponent():base("Midas Static Load Case","Load Case","Create a static load case.","06-Cases"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Case database key.",GH_ParamAccess.item,1);p.AddTextParameter("Name","N","Case name.",GH_ParamAccess.item,"LC1");p.AddTextParameter("Type","T","Native case type: D, L, USER, etc.",GH_ParamAccess.item,"USER");}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Case","C","Case definition.",GH_ParamAccess.item);p.AddTextParameter("Name","N","Connect to loads.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){Output(da,0,Definitions.LoadCase(Item<int>(da,0),Item<string>(da,1),Item<string>(da,2)));da.SetData(1,Item<string>(da,1));}
}
public sealed class LoadCombinationComponent:SafeComponent
{
    public LoadCombinationComponent():base("Midas Load Combination","Combination","Linear/envelope/absolute/SRSS combination of static cases. Native API component supports other analysis types.","06-Cases"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Combination ID.",GH_ParamAccess.item,1);p.AddTextParameter("Name","N","Combination name.",GH_ParamAccess.item,"ULS");p.AddTextParameter("Cases","C","Static case names.",GH_ParamAccess.list);p.AddNumberParameter("Factors","F","One factor per case.",GH_ParamAccess.list);p.AddIntegerParameter("Type","T","0 Linear, 1 Envelope, 2 Absolute, 3 SRSS.",GH_ParamAccess.item,0);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Combination","C","Native definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var c=new List<string>();var f=new List<double>();da.GetDataList(2,c);da.GetDataList(3,f);Output(da,0,Definitions.Combination(Item<int>(da,0),Item<string>(da,1),c,f,Item<int>(da,4)));}
}
