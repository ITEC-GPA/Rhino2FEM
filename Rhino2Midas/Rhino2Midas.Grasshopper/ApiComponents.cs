using System.Text.Json;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Rhino2Midas.Core;

namespace Rhino2Midas.Grasshopper;

public abstract class NativeDefinitionComponent:SafeComponent
{
    protected abstract string Key{get;}
    protected NativeDefinitionComponent(string title,string nickname,string category):base(title,nickname,"Native Civil NX API definition. Values start from the official MIDAS example; edit IDs, references and dimensions for your model. No network call until Export/Analyze.",category){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddIntegerParameter("ID","ID","Native database ID, or target node/element ID for assignments.",GH_ParamAccess.item,1);
        p.AddGenericParameter("Dependencies","D","Related materials, sections, cases or element fragments collected in the Model.",GH_ParamAccess.list);p[1].Optional=true;
        foreach(var field in ApiCatalogue.Get(Key).Example.EnumerateObject())
        {
            string help="Native "+field.Name+". Initial value is the official documentation example.";
            switch(field.Value.ValueKind)
            {
                case JsonValueKind.True:case JsonValueKind.False:p.AddBooleanParameter(field.Name,field.Name,help,GH_ParamAccess.item,field.Value.GetBoolean());break;
                case JsonValueKind.Number:p.AddNumberParameter(field.Name,field.Name,help,GH_ParamAccess.item,field.Value.GetDouble());break;
                case JsonValueKind.String:p.AddTextParameter(field.Name,field.Name,help,GH_ParamAccess.item,field.Value.GetString());break;
                default:p.AddTextParameter(field.Name,field.Name,help+" Supply a JSON array/object in a Panel.",GH_ParamAccess.item,field.Value.GetRawText());break;
            }
        }
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Definition","D","Immutable native definition plus dependencies.",GH_ParamAccess.item);p.AddTextParameter("JSON","J","Record sent to Civil under Assign/ID.",GH_ParamAccess.item);p.AddTextParameter("Reference","R","Official MIDAS documentation.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da)
    {
        var definition=ApiCatalogue.Get(Key);var data=new JsonObject();int i=2;
        foreach(var field in definition.Example.EnumerateObject())
        {
            data[field.Name]=field.Value.ValueKind switch{JsonValueKind.True or JsonValueKind.False=>JsonValue.Create(Item<bool>(da,i)),JsonValueKind.Number=>JsonValue.Create(Item<double>(da,i)),JsonValueKind.String=>JsonValue.Create(Item<string>(da,i)),_=>JsonNode.Parse(Item<string>(da,i))};i++;
        }
        var json=JsonSerializer.SerializeToElement(data);var errors=ApiCatalogue.Validate(json,definition.Schema).ToArray();if(errors.Length>0)throw new ArgumentException(string.Join("; ",errors));
        var deps=new List<object>();da.GetDataList(1,deps);var fragment=Fragment.Combine(deps.Select(FragmentOf)).Append(new(definition.Endpoint[3..],Item<int>(da,0),json,definition.Item));
        Output(da,0,fragment);da.SetData(1,json.GetRawText());da.SetData(2,definition.Url);
    }
}
public abstract class NativeResultRequestComponent:SafeComponent
{
    protected abstract string Key{get;}
    protected NativeResultRequestComponent(string title):base(title,"Result request","Request a native Civil result table during Analyze. Exact column names and options are preserved.","15-Native Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddIntegerParameter("IDs","ID","Native node/element IDs. Empty omits NODE_ELEMS for global tables.",GH_ParamAccess.list);p[0].Optional=true;
        p.AddTextParameter("Cases","C","Exact case identifiers, e.g. LC1(ST), ULS(CB), Summation(CS). Empty for eigenvalue/global tables.",GH_ParamAccess.list);p[1].Optional=true;
        p.AddTextParameter("Options JSON","O","Official native request options. Units, IDs, cases and table name are replaced by the connected Model/request.",GH_ParamAccess.item,ApiCatalogue.Get(Key).Example.GetRawText());
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Request","R","Connect to Analyze/Read Result Table.",GH_ParamAccess.item);p.AddTextParameter("Reference","H","Official MIDAS reference.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da)
    {
        var ids=new List<int>();var cases=new List<string>();da.GetDataList(0,ids);da.GetDataList(1,cases);using var doc=JsonDocument.Parse(Item<string>(da,2));var options=doc.RootElement.Clone();
        var request=new TableRequest(options.GetProperty("TABLE_TYPE").GetString()!,options.GetProperty("COMPONENTS").EnumerateArray().Select(v=>v.GetString()!).ToArray(),ids,cases,Options:options);
        da.SetData(0,request);da.SetData(1,ApiCatalogue.Get(Key).Url);
    }
}
public sealed class NativeRecordComponent:SafeComponent
{
    public NativeRecordComponent():base("Midas Native Database Record","API Record","Build any Civil /db record as an immutable definition. No network request.","14-Native API"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Endpoint","E","Database name, e.g. SECT, TDN-PROPERTY.",GH_ParamAccess.item);p.AddIntegerParameter("ID","ID","Native key.",GH_ParamAccess.item,1);p.AddTextParameter("JSON","J","One native record, without Assign wrapper.",GH_ParamAccess.item);p.AddBooleanParameter("Item","I","Record is one item in the target's ITEMS list.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Definition","D","Connect to Build Model.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){using var doc=JsonDocument.Parse(Item<string>(da,2));Output(da,0,new Fragment(entries:[new(Item<string>(da,0),Item<int>(da,1),doc.RootElement,Item<bool>(da,3))]));}
}
