using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Midas.Core;

namespace Rhino2Midas.Grasshopper;

public sealed class SupportComponent:SafeComponent
{
    public SupportComponent():base("Midas Node Support","Support","Assign translational/rotational restraints to copies of nodes.","04-Attributes"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Nodes","N","Node fragment.",GH_ParamAccess.item);p.AddTextParameter("Mask","M","Dx Dy Dz Rx Ry Rz [Rw], 1 restrained.",GH_ParamAccess.item,"1111110");p.AddTextParameter("Group","G","Boundary group name or blank.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Nodes","N","Nodes with supports.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.Support(Native<Fragment>(da,0),Item<string>(da,1),Item<string>(da,2)));
}
public sealed class NodeSpringComponent:SafeComponent
{
    public NodeSpringComponent():base("Midas Node Spring","Spring","Linear spring stiffness F/L, F·L/rad.","04-Attributes"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Nodes","N","Node fragment.",GH_ParamAccess.item);p.AddNumberParameter("Stiffness","K","Six nonnegative values.",GH_ParamAccess.list);p.AddTextParameter("Group","G","Boundary group.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Nodes","N","Modified copy.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var k=new List<double>();da.GetDataList(1,k);Definitions.Six(k,true);Output(da,0,Definitions.Attribute(Native<Fragment>(da,0),ElementKind.Node,"NSPR",new{TYPE="LINEAR",F_S=new bool[6],SDR=k,DAMPING=false,GROUP_NAME=Item<string>(da,2)}));}
}
public sealed class NodeMassComponent:SafeComponent
{
    public NodeMassComponent():base("Midas Node Mass","Mass","Lumped masses in consistent Model force/length/time units.","04-Attributes"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Nodes","N","Node fragment.",GH_ParamAccess.item);p.AddNumberParameter("Masses","M","Six masses/inertias: mX mY mZ rmX rmY rmZ.",GH_ParamAccess.list);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Nodes","N","Modified copy.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var m=new List<double>();da.GetDataList(1,m);Definitions.Six(m,true);Output(da,0,Definitions.Attribute(Native<Fragment>(da,0),ElementKind.Node,"NMAS",new{mX=m[0],mY=m[1],mZ=m[2],rmX=m[3],rmY=m[4],rmZ=m[5]},false));}
}
public sealed class BeamReleaseComponent:SafeComponent
{
    public BeamReleaseComponent():base("Midas Beam Releases","Releases","Native beam end release flags. Zero partial-fixity values produce full releases.","04-Attributes"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Beam","B","Beam fragment.",GH_ParamAccess.item);p.AddTextParameter("I mask","I","1 released, Fx Fy Fz Mx My Mz [Mw].",GH_ParamAccess.item,"0000000");p.AddTextParameter("J mask","J","1 released.",GH_ParamAccess.item,"0000000");p.AddTextParameter("Group","G","Boundary group.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Beam","B","Modified copy.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.Attribute(Native<Fragment>(da,0),ElementKind.Beam,"FRLS",new{GROUP_NAME=Item<string>(da,3),bVALUE=true,FLAG_I=Definitions.Mask(Item<string>(da,1)),VALUE_I=new double[7],FLAG_J=Definitions.Mask(Item<string>(da,2)),VALUE_J=new double[7]}));
}
public sealed class BeamOffsetComponent:SafeComponent
{
    public BeamOffsetComponent():base("Midas Beam End Offset","Offset","Offsets in global coordinates, shown in physical preview.","04-Attributes"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Beam","B","Beam fragment.",GH_ParamAccess.item);p.AddVectorParameter("I offset","I","Global offset vector at I.",GH_ParamAccess.item,Vector3d.Zero);p.AddVectorParameter("J offset","J","Global offset vector at J.",GH_ParamAccess.item,Vector3d.Zero);p.AddTextParameter("Group","G","Boundary group.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Beam","B","Modified copy.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var i=Item<Vector3d>(da,1);var j=Item<Vector3d>(da,2);Output(da,0,Definitions.Attribute(Native<Fragment>(da,0),ElementKind.Beam,"OFFS",new{GROUP_NAME=Item<string>(da,3),TYPE="GLOBAL",RGDXi=i.X,RGDYi=i.Y,RGDZi=i.Z,RGDXj=j.X,RGDYj=j.Y,RGDZj=j.Z}));}
}
public sealed class NodalLoadComponent:SafeComponent
{
    public NodalLoadComponent():base("Midas Nodal Load","Nodal Load","Nodal forces F and moments F·L, in native nodal axes.","05-Loads"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Nodes","N","Node fragment.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Static load case name.",GH_ParamAccess.item,"LC1");p.AddNumberParameter("Loads","F","FX FY FZ MX MY MZ.",GH_ParamAccess.list);p.AddTextParameter("Group","G","Load group.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Nodes","N","Nodes and loads.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var f=new List<double>();da.GetDataList(2,f);Definitions.Six(f);Output(da,0,Definitions.Attribute(Native<Fragment>(da,0),ElementKind.Node,"CNLD",new{LCNAME=Item<string>(da,1),GROUP_NAME=Item<string>(da,3),FX=f[0],FY=f[1],FZ=f[2],MX=f[3],MY=f[4],MZ=f[5]}));}
}
public abstract class BeamLoadBase:SafeComponent
{
    protected abstract string LoadType{get;}
    protected BeamLoadBase(string name):base("Midas "+name,name,"Native beam load. Positions are relative distances 0..1; no implicit unit conversion.","05-Loads"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Beam","B","Beam fragment.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Load case name.",GH_ParamAccess.item,"LC1");p.AddTextParameter("Direction","D","GX GY GZ LX LY LZ.",GH_ParamAccess.item,"GZ");p.AddNumberParameter("Start value","P1","Force/length for distributed loads; force for point loads; corresponding moment units for moment types.",GH_ParamAccess.item,-10);p.AddNumberParameter("End value","P2","Second value for distributed load.",GH_ParamAccess.item,-10);p.AddNumberParameter("Start position","D1","Relative position.",GH_ParamAccess.item,0);p.AddNumberParameter("End position","D2","Relative position.",GH_ParamAccess.item,1);p.AddBooleanParameter("Projected","P","Project distributed loads.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Beam","B","Beam and load.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){double d1=Item<double>(da,5),d2=Item<double>(da,6);bool point=LoadType.StartsWith("CON");if(d1<0||d1>1||d2<0||d2>1||(!point&&d2<=d1))throw new ArgumentException("Load positions must lie in 0..1 with start < end for distributed loads.");string dir=Item<string>(da,2).ToUpperInvariant();if(!new[]{"GX","GY","GZ","LX","LY","LZ"}.Contains(dir))throw new ArgumentException("Invalid load direction.");Output(da,0,Definitions.Attribute(Native<Fragment>(da,0),ElementKind.Beam,"BMLD",new{LCNAME=Item<string>(da,1),GROUP_NAME="",CMD="BEAM",TYPE=LoadType,DIRECTION=dir,USE_PROJECTION=!point&&Item<bool>(da,7),USE_ECCEN=false,D=new[]{d1,point?0:d2,0,0},P=new[]{Item<double>(da,3),point?0:Item<double>(da,4),0,0}}));}
}
public sealed class BeamDistributedLoadComponent:BeamLoadBase{protected override string LoadType=>"UNILOAD";public BeamDistributedLoadComponent():base("Beam Distributed Load"){} }
public sealed class BeamPointLoadComponent:BeamLoadBase{protected override string LoadType=>"CONLOAD";public BeamPointLoadComponent():base("Beam Point Load"){} }
public sealed class BeamPointMomentComponent:BeamLoadBase{protected override string LoadType=>"CONMOMENT";public BeamPointMomentComponent():base("Beam Point Moment"){} }
public sealed class BeamDistributedMomentComponent:BeamLoadBase{protected override string LoadType=>"UNIMOMENT";public BeamDistributedMomentComponent():base("Beam Distributed Moment"){} }
public sealed class PlatePressureComponent:SafeComponent
{
    public PlatePressureComponent():base("Midas Plate Pressure","Pressure","Uniform pressure on plate faces.","05-Loads"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Plate","P","Plate fragment.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Load case.",GH_ParamAccess.item,"LC1");p.AddNumberParameter("Pressure","F","F/L².",GH_ParamAccess.item,-10);p.AddTextParameter("Direction","D","Native LZ or global direction.",GH_ParamAccess.item,"LZ");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Plate","P","Plate and pressure.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.Attribute(Native<Fragment>(da,0),ElementKind.Plate,"PRES",new{LCNAME=Item<string>(da,1),GROUP_NAME="",CMD="PRES",ELEM_TYPE="PLATE",FACE_EDGE_TYPE="FACE",DIRECTION=Item<string>(da,3),FORCES=new[]{Item<double>(da,2),0,0,0,0}}));
}
public sealed class SelfWeightComponent:SafeComponent
{
    public SelfWeightComponent():base("Midas Self Weight","Self Weight","Global multipliers of material weight.","05-Loads"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Self weight record ID.",GH_ParamAccess.item,1);p.AddTextParameter("Case","C","Case name.",GH_ParamAccess.item,"LC1");p.AddVectorParameter("Factors","F","Global weight multipliers.",GH_ParamAccess.item,new Vector3d(0,0,-1));}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Load","L","Self weight definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var v=Item<Vector3d>(da,2);Output(da,0,Definitions.Record("BODF",Item<int>(da,0),new{LCNAME=Item<string>(da,1),GROUP_NAME="",FV=new[]{v.X,v.Y,v.Z}}));}
}
