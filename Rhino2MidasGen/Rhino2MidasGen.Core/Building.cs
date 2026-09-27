using System.Text.Json;

namespace Rhino2MidasGen.Core;

public static class Building
{
    public static int WallId(Element e)=>e.Kind==ElementKind.Wall?(int)Native.Number(JsonSerializer.Deserialize<JsonElement>(e.Extra),"WALL"):0;
    public static string WallData(int wallId,int subtype=2,int formulation=0)
    {
        if(wallId<=0||subtype is not (1 or 2)||formulation is <0 or >2)throw new ArgumentException("Wall ID must be positive; STYPE is 1 membrane or 2 plate; W_TYPE is 0 plate base, 1 CRB pin or 2 CRB fixed.");
        return JsonSerializer.Serialize(new{STYPE=subtype,WALL=wallId,W_CON=0,W_TYPE=formulation});
    }
    public static Fragment Story(int id,string name,double level,bool diaphragm=false)
    {
        if(id<=0||!double.IsFinite(level))throw new ArgumentException("Story needs a positive ID and finite level.");
        return Definitions.Record("STOR",id,new{STORY_NAME=Native.Name(name),STORY_LEVEL=level,bFLOOR_DIAPHRAGM=diaphragm,
            WIND_FLOOR_WIDTH_X=0d,WIND_FLOOR_WIDTH_Y=0d,WIND_CENTER_X=0d,WIND_CENTER_Y=0d,WIND_ECCENT_X=0d,WIND_ECCENT_Y=0d,
            SEIS_ACC_ECCENT_X=0d,SEIS_ACC_ECCENT_Y=0d,SEIS_INHERENT_ECCENT_X=0d,SEIS_INHERENT_ECCENT_Y=0d,SEIS_TORSIONAL_AMP_FACTOR_X=1d,SEIS_TORSIONAL_AMP_FACTOR_Y=1d});
    }
    public static MidasModel Example()
    {
        var wall=new Element(101,ElementKind.Wall,[new(0,0,0),new(4,0,0),new(4,0,3),new(0,0,3)],1,1,nodeIds:[1,2,3,4],extra:WallData(7));
        var nodes=wall.Points.Select((p,i)=>new Element(i+1,ElementKind.Node,[p])).ToArray();
        return new(Fragment.Combine([Definitions.Material(1,"Concrete",30000000,.2,25,type:"CONC"),Definitions.Thickness(1,.2),
            new Fragment(nodes.Append(wall).ToArray()),Story(1,"Base",0),Story(2,"Roof",3),Definitions.LoadCase(1,"Lateral"),
            Definitions.Record("CONS",1,new{CONSTRAINT="1111110",GROUP_NAME=""},true),Definitions.Record("CONS",2,new{CONSTRAINT="1111110",GROUP_NAME=""},true),
            Definitions.Record("CNLD",3,new{LCNAME="Lateral",FX=5d,FY=0d,FZ=0d,MX=0d,MY=0d,MZ=0d,GROUP_NAME=""},true),
            Definitions.Record("CNLD",4,new{LCNAME="Lateral",FX=5d,FY=0d,FZ=0d,MX=0d,MY=0d,MZ=0d,GROUP_NAME=""},true)]));
    }
}
