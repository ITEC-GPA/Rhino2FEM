using System.Net;
using System.Text;
using System.Text.Json;
using Rhino2Midas.Api;
using Rhino2Midas.Core;

internal static class ImportChecks
{
    internal static async Task Run(Action<bool,string> check)
    {
        var source=Definitions.Example();
        var records=ApiModel.Writes(source).ToDictionary(w=>w.Endpoint,w=>JsonSerializer.SerializeToElement(w.Records),StringComparer.OrdinalIgnoreCase);
        records["STYP"]=JsonSerializer.SerializeToElement(new Dictionary<int,object>{{1,new{STYP=0,GRAV=9.81,TEMP=17}}});
        records["GRUP"]=JsonSerializer.SerializeToElement(new Dictionary<int,object>{{1,new{NAME="Imported group",N_LIST=new[]{1,2},E_LIST=new[]{1}}}});
        records["CUSTOM"]=JsonSerializer.SerializeToElement(new Dictionary<int,object>{{7,new{NAME="Keep native extension",VALUE=12.5}}});
        using var handler=new ImportHandler(records);
        using var client=new CivilClient("https://mock.invalid/civil/","import-secret",handler);
        var result=await CivilService.ReadExistingModelAsync(client,["custom"," CUSTOM ","MATL","NODE"]);
        check(handler.Calls.All(c=>c.Method=="GET"),"Native import uses only GET; no new/save/analysis requests");
        check(handler.Calls.Count(c=>c.Table=="MATL")==1&&handler.Calls.Count(c=>c.Table=="NODE")==1&&handler.Calls.Count(c=>c.Table=="CUSTOM")==1,"Default/additional tables normalized and read once");
        check(ModelValidation.Errors(result.Model).Count==0,"Default native import reconstructs a valid associated Model");
        var roundtrip=ApiModel.Writes(result.Model).ToDictionary(w=>w.Endpoint);
        check(new[]{"MATL","SECT","CNLD","CONS","STLD"}.All(t=>Native.Canonical(JsonSerializer.SerializeToElement(roundtrip[t].Records))==Native.Canonical(records[t])),"Native material, section, load, restraint and case values preserved");
        check(roundtrip["STYP"].Records[1].GetProperty("GRAV").GetDouble()==9.81,"Imported gravity overrides default model gravity");
        check(roundtrip["CUSTOM"].Records[7].GetProperty("VALUE").GetDouble()==12.5&&roundtrip.ContainsKey("GRUP"),"Additional native records and groups retained");
        check(result.Issues.Any(s=>s.StartsWith("THIK:"))&&result.Issues.Any(s=>s.Contains("Snapshot")),"Missing optional table and partial snapshot scope visible");
        check(result.Model.Results==null,"Import never invents solver results");
        foreach(var status in new[]{HttpStatusCode.Unauthorized,HttpStatusCode.Forbidden})
        {
            using var denied=new CivilClient("https://mock.invalid/civil/","import-secret",new ImportHandler(records,"MATL",status));
            try{await CivilService.ReadExistingModelAsync(denied,[]);throw new Exception("Authentication failure was swallowed");}
            catch(HttpRequestException ex){check(ex.StatusCode==status&&!ex.Message.Contains("import-secret"),"Authentication errors stop import and redact API key: "+status);}
        }
        using var mandatory=new CivilClient("https://mock.invalid/civil/","import-secret",new ImportHandler(records,"NODE",HttpStatusCode.NotFound));
        try{await CivilService.ReadExistingModelAsync(mandatory,[]);throw new Exception("Mandatory geometry failure was swallowed");}
        catch(HttpRequestException ex){check(ex.StatusCode==HttpStatusCode.NotFound,"Missing mandatory geometry prevents publishing partial Model");}
        using var transport=new CivilClient("https://mock.invalid/civil/","import-secret",new ImportHandler(records,disconnect:true));
        try{await CivilService.ReadExistingModelAsync(transport,[]);throw new Exception("Transport failure was swallowed");}
        catch(HttpRequestException ex){check(ex.StatusCode==null,"Connection loss during optional tables aborts import");}

    }
    private sealed class ImportHandler(Dictionary<string,JsonElement> records,string? fail=null,HttpStatusCode status=HttpStatusCode.NotFound,bool disconnect=false):HttpMessageHandler
    {
        internal List<(string Method,string Table)> Calls {get;}=[];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            string table=request.RequestUri!.Segments.Last().ToUpperInvariant();
            Calls.Add((request.Method.Method,table));
            if(disconnect&&table=="MATL")throw new HttpRequestException("Disconnected");
            bool exists=records.TryGetValue(table,out var value)&&table!=fail;
            var response=new HttpResponseMessage(exists?HttpStatusCode.OK:table==fail?status:HttpStatusCode.NotFound)
            {Content=new StringContent(exists?JsonSerializer.Serialize(new Dictionary<string,JsonElement>{{table,value}}):"{\"error\":{\"message\":\"import-secret unavailable\"}}",Encoding.UTF8,"application/json")};
            return Task.FromResult(response);
        }
    }
}
