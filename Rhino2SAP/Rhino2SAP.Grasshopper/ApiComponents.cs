using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public abstract class ApiCommandComponent:SafeComponent
{
    protected abstract string MethodKey { get; }
    private ApiMethod Method=>ApiSchema.Get(MethodKey);
    protected ApiCommandComponent(string key,string title):base(title,title,"Deferred SAP2000 command: "+key+". All dimensional values use Model units. Connect named definitions to name/reference sockets to collect their dependencies.",ComponentTopics.ForApi(key)){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Definitions","D","Prior definitions/elements. Connected fragments are carried into the output.",GH_ParamAccess.list);p[0].Optional=true;
        foreach(var a in Method.Parameters)
        {
            int index;
            var access=a.IsArray?GH_ParamAccess.list:GH_ParamAccess.item;
            if(a.Type=="System.String")index=p.AddGenericParameter(a.Name,a.Name,a.Description+". Text or one named SAP definition/element.",access);
            else if(a.ScalarType=="System.Boolean")index=a.Optional&&!a.IsArray?p.AddBooleanParameter(a.Name,a.Name,a.Description,access,a.Default.GetBoolean()):p.AddBooleanParameter(a.Name,a.Name,a.Description,access);
            else if(a.ScalarType=="System.Double")index=a.Optional&&!a.IsArray?p.AddNumberParameter(a.Name,a.Name,a.Description,access,a.Default.GetDouble()):p.AddNumberParameter(a.Name,a.Name,a.Description,access);
            else if(a.ScalarType=="System.String")index=p.AddTextParameter(a.Name,a.Name,a.Description,access);
            else index=a.Optional&&!a.IsArray?p.AddIntegerParameter(a.Name,a.Name,a.Description,access,a.Default.GetInt32()):p.AddIntegerParameter(a.Name,a.Name,a.Description,access);
            if(a.Optional)p[index].Optional=true;
            // Generic name/reference sockets also show the actual SDK string default.
            if(a.Optional&&a.Type=="System.String"&&a.Default.ValueKind==JsonValueKind.String)
                ((global::Grasshopper.Kernel.Parameters.Param_GenericObject)p[index]).PersistentData.Append(new GH_String(a.Default.GetString()!));
        }
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {p.AddGenericParameter("Definition","D","SAP fragment including dependencies and this operation.",GH_ParamAccess.item);p.AddTextParameter("Name","N","Primary SAP name, where applicable.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da)
    {
        var prior=new List<object>();da.GetDataList(0,prior);var fragments=prior.Select(FragmentOf).ToList();var args=new Dictionary<string,JsonElement>();
        for(int i=0;i<Method.Parameters.Length;i++)
        {
            var p=Method.Parameters[i];object? value;
            if(p.IsArray){var list=new List<object>();bool present=da.GetDataList(i+1,list);if(!present&&p.Optional)continue;value=list.Select(Unwrap).ToArray();}
            else
            {
                object raw=null!;if(!da.GetData(i+1,ref raw)){if(p.Optional)continue;throw new ArgumentException($"Missing {p.Name}.");}
                value=Unwrap(raw);
                if(value is Fragment fragment)
                {
                    fragments.Add(fragment);var names=fragment.Elements.Select(e=>e.Name).Concat(fragment.Operations.Where(o=>o.Arguments.ContainsKey("Name")).Select(o=>o.Arguments["Name"].GetString()!)).Distinct().ToArray();
                    // The last operation's primary name identifies a named property with material dependencies.
                    value=fragment.Elements.Count==1?fragment.Elements[0].Name:fragment.Operations.LastOrDefault(o=>o.Arguments.ContainsKey("Name"))?.Arguments["Name"].GetString()??(names.Length==1?names[0]:throw new ArgumentException("Connect one named definition, or a text name."));
                }
                if(p.Type=="System.String")value=Convert.ToString(value,System.Globalization.CultureInfo.InvariantCulture)??"";
            }
            args[p.Name]=ApiSchema.Value(value);
        }
        var op=new Operation(MethodKey,args);var combined=Fragment.Combine(fragments).Append(op);Output(da,0,combined);
        if(args.TryGetValue("Name",out var name))da.SetData(1,name.GetString());
    }
}

public abstract class ApiResultComponent:SafeComponent
{
    protected abstract string MethodKey { get; }
    protected ApiResultComponent(string key,string title):base(title,title,"Read stored "+key+" results. Native column names, signs, axes and sampling are preserved. Request this method when running SAP.",ComponentTopics.ForApi(key)){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {p.AddGenericParameter("Model","M","Solved SAP Model.",GH_ParamAccess.item);p.AddTextParameter("Object","O","Optional native object name filter (Obj column).",GH_ParamAccess.item,"");p.AddTextParameter("Case","C","Optional load case/combination filter.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        foreach(var a in ApiSchema.Get(MethodKey).Parameters.Where(a=>a.ByRef))
        {var access=a.IsArray?GH_ParamAccess.list:GH_ParamAccess.item;if(a.ScalarType=="System.String")p.AddTextParameter(a.Name,a.Name,a.Name,access);else if(a.ScalarType=="System.Double")p.AddNumberParameter(a.Name,a.Name,a.Name,access);else p.AddIntegerParameter(a.Name,a.Name,a.Name,access);}
    }
    protected override void Solve(IGH_DataAccess da)
    {
        var model=Native<SapModel>(da,0);var results=model.Results??throw new ArgumentException("Run this Model first, or load its result archive.");results.RequireValidFor(model);
        var table=results.Tables.SingleOrDefault(t=>t.Method==MethodKey)??throw new ArgumentException("Quantity not captured. Add "+MethodKey+" to Run Model's Quantities input.");
        string name=Item<string>(da,1),loadCase=Item<string>(da,2);
        var rows=Enumerable.Range(0,table.RowCount).Where(i=>Match(table,"Obj",name,i)&&Match(table,"LoadCase",loadCase,i)).ToArray();int output=0;
        foreach(var p in ApiSchema.Get(MethodKey).Parameters.Where(p=>p.ByRef))
        {
            if(table.Columns.TryGetValue(p.Name,out var value))
            {if(value.ValueKind==JsonValueKind.Array){var all=value.EnumerateArray().ToArray();da.SetDataList(output,rows.Where(i=>i<all.Length).Select(i=>Scalar(all[i])));}else da.SetData(output,p.Name=="NumberResults"?rows.Length:Scalar(value));}output++;
        }
    }
    public static object? Scalar(JsonElement e)=>e.ValueKind switch{JsonValueKind.String=>e.GetString(),JsonValueKind.Number=>e.GetDouble(),JsonValueKind.True=>true,JsonValueKind.False=>false,_=>null};
    private static bool Match(ResultTable t,string col,string filter,int row)=>string.IsNullOrEmpty(filter)||t.Columns.TryGetValue(col,out var a)&&a.ValueKind==JsonValueKind.Array&&row<a.GetArrayLength()&&a[row].GetString()==filter;
}
