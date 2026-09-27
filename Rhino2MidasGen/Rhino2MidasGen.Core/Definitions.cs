using System.Text.Json;

namespace Rhino2MidasGen.Core;

public static class Definitions
{
    public static Fragment Record(string endpoint,int id,object data,bool item=false)=>new(entries:[ApiEntry.Create(endpoint,id,data,item)]);
    public static void Positive(params double[] values){if(values.Any(v=>!double.IsFinite(v)||v<=0))throw new ArgumentException("Values must be positive and finite.");}
    public static void Six(IReadOnlyList<double> values,bool nonnegative=false){if(values.Count!=6||values.Any(v=>!double.IsFinite(v)||(nonnegative&&v<0)))throw new ArgumentException("Expected six finite values: X Y Z RX RY RZ.");}
    public static string Mask(string mask){if(mask.Length==6)mask+="0";if(mask.Length!=7||mask.Any(c=>c!='0'&&c!='1'))throw new ArgumentException("DOF mask must have six or seven 0/1 digits: Dx Dy Dz Rx Ry Rz [Rw].");return mask;}
    public static Fragment Material(int id,string name,double e,double poisson,double weight,double thermal=1.2e-5,string type="USER")
    {
        Positive(e);if(poisson<=-1||poisson>=.5||!double.IsFinite(poisson)||weight<0||!double.IsFinite(weight)||!double.IsFinite(thermal))throw new ArgumentException("Invalid isotropic material.");
        if(!new[]{"STEEL","CONC","USER"}.Contains(type))throw new ArgumentException("Material type: STEEL, CONC or USER.");
        return Record("MATL",id,new{TYPE=type,NAME=Native.Name(name),HE_SPEC=0,HE_COND=0,PLMT=0,P_NAME="",bMASS_DENS=false,DAMP_RAT=.02,PARAM=new[]{new{P_TYPE=2,ELAST=e,POISN=poisson,THERMAL=thermal,DEN=weight,MASS=0d}}});
    }
    public static Fragment DatabaseMaterial(int id,string name,string type,string standard,string grade)=>Record("MATL",id,new{TYPE=type,NAME=Native.Name(name),bMASS_DENS=false,PARAM=new[]{new{P_TYPE=1,STANDARD=Native.Name(standard),CODE="",DB=Native.Name(grade),bELAST=false}}});
    public static Fragment Section(int id,string name,string shape,IReadOnlyList<double> dimensions,string offset="CC")
    {
        var expected=new Dictionary<string,int>{{"SB",2},{"SR",1},{"P",2},{"B",4},{"H",6},{"T",4},{"C",4},{"L",4}};
        if(!expected.TryGetValue(shape,out int count)||dimensions.Count<count||dimensions.Count>10)throw new ArgumentException("Invalid section shape or dimension count.");
        Positive(dimensions.Take(count).ToArray());if(dimensions.Skip(count).Any(d=>!double.IsFinite(d)||d<0))throw new ArgumentException("Section radii cannot be negative.");
        if(!new[]{"LT","CT","RT","LC","CC","RC","LB","CB","RB"}.Contains(offset))throw new ArgumentException("Invalid section insertion point.");
        var d=dimensions.ToList();while(d.Count<10)d.Add(0);
        if(shape is "B" or "C"&&dimensions.Count==4){d[4]=d[1];d[5]=d[3];}
        if(shape=="P"&&2*d[1]>=d[0])throw new ArgumentException("Pipe wall must be smaller than radius.");
        if(shape=="B"&&(2*d[2]>=d[1]||2*d[3]>=d[0]))throw new ArgumentException("Box walls leave no interior.");
        if(shape=="H"&&(d[3]+d[5]>=d[0]||d[2]>=Math.Min(d[1],d[4])))throw new ArgumentException("Invalid I section web/flanges.");
        if(shape is "T" or "C" or "L"&&(d[2]>=d[1]||d[3]>=d[0]||(shape=="C"&&2*d[3]>=d[0])))throw new ArgumentException("Invalid open section thickness.");
        return Record("SECT",id,new{SECTTYPE="DBUSER",SECT_NAME=Native.Name(name),SECT_BEFORE=new{OFFSET_PT=offset,OFFSET_CENTER=0,USER_OFFSET_REF=0,HORZ_OFFSET_OPT=0,USERDEF_OFFSET_YI=0,VERT_OFFSET_OPT=0,USERDEF_OFFSET_ZI=0,USE_SHEAR_DEFORM=true,USE_WARPING_EFFECT=false,SHAPE=shape,DATATYPE=2,SECT_I=new{vSIZE=d}}});
    }
    public static Fragment DatabaseSection(int id,string name,string shape,string database,string section)=>Record("SECT",id,new{SECTTYPE="DBUSER",SECT_NAME=Native.Name(name),SECT_BEFORE=new{OFFSET_PT="CC",USE_SHEAR_DEFORM=true,USE_WARPING_EFFECT=false,SHAPE=Native.Name(shape),DATATYPE=1,SECT_I=new{DB_NAME=Native.Name(database),SECT_NAME=Native.Name(section)}}});
    public static Fragment Thickness(int id,double membrane,double bending=0,double offset=0)
    {Positive(membrane);if(bending<0||!double.IsFinite(bending)||!double.IsFinite(offset))throw new ArgumentException("Invalid thickness/offset.");return Record("THIK",id,new{NAME=id.ToString(),TYPE="VALUE",bINOUT=bending!=0,T_IN=membrane,T_OUT=bending,OFFSET=offset==0?0:2,O_VALUE=offset});}
    public static Fragment LoadCase(int id,string name,string type="USER",string description="")=>Record("STLD",id,new{NAME=Native.Name(name),TYPE=Native.Name(type),DESC=description});
    public static Fragment Combination(int id,string name,IReadOnlyList<string> cases,IReadOnlyList<double> factors,int type=0)
    {
        if(cases.Count==0||cases.Count!=factors.Count||type is <0 or >3||factors.Any(f=>!double.IsFinite(f)))throw new ArgumentException("Combination needs matching case/factor lists and native type 0..3.");
        return Record("LCOM-GEN",id,new{NAME=Native.Name(name),ACTIVE="ACTIVE",iTYPE=type,vCOMB=cases.Select((c,i)=>new{ANAL="ST",LCNAME=Native.Name(c),FACTOR=factors[i]}).ToArray()});
    }
    public static Fragment Support(Fragment nodes,string mask,string group="")=>Attribute(nodes,ElementKind.Node,"CONS",new{CONSTRAINT=Mask(mask),GROUP_NAME=group});
    public static Fragment Attribute(Fragment input,ElementKind? kind,string endpoint,object data,bool item=true)
    {
        var targets=input.Elements.Where(e=>kind==null||e.Kind==kind).ToArray();if(targets.Length==0)throw new ArgumentException("No compatible target elements.");
        return new(input.Elements.Select(e=>e with{Results=[]}).ToArray(),input.Entries.Concat(targets.Select(e=>ApiEntry.Create(endpoint,e.Id,data,item))).ToArray());
    }
    public static int PropertyId(Fragment definition,string endpoint)
    {
        var ids=definition.Entries.Where(e=>e.Endpoint==endpoint).Select(e=>e.Id).Distinct().ToArray();
        return ids.Length==1?ids[0]:throw new ArgumentException($"Connect exactly one {endpoint} definition, or a numeric ID.");
    }
    public static MidasModel Example()
    {
        var geometry=new Fragment([new(1,ElementKind.Node,[new(0,0,0)]),new(2,ElementKind.Node,[new(3,0,0)]),new(1,ElementKind.Beam,[new(0,0,0),new(3,0,0)],1,1,nodeIds:[1,2])]);
        return new(Fragment.Combine([Material(1,"Steel",210000000,.3,78.5,type:"STEEL"),Section(1,"R200x300","SB",[.3,.2]),geometry,LoadCase(1,"LC1"),Record("CONS",1,new{CONSTRAINT="1111110",GROUP_NAME=""},true),Record("CNLD",2,new{LCNAME="LC1",GROUP_NAME="",FX=0,FY=0,FZ=-10,MX=0,MY=0,MZ=0},true)]));
    }
}
