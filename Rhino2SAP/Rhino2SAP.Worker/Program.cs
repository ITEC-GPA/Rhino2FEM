using System.Text.Json;
using Rhino2SAP.Core;
using Rhino2SAP.Api;

if(args.Length!=2){Console.Error.WriteLine("Rhino2SAP.Worker request.json response.json");return 2;}
SapClient.Trace=Console.WriteLine;
WorkerResponse response;
try
{
    var request=JsonSerializer.Deserialize<WorkerRequest>(File.ReadAllText(args[0]))??throw new InvalidDataException("Empty worker request.");
    string data=SapThread.Run(()=>request.Command switch
    {
        "Run"=>ModelArchive.Serialize(new AnalysisService().Execute(ModelArchive.Deserialize(request.ModelJson!),request.Options!)),
        "Geometry"=>ModelArchive.Serialize(new SapModel(ReadService.Geometry(request.File!))),
        "ImportModel"=>ModelArchive.Serialize(new ModelImportService().Read(request.File!)),
        "Query"=>JsonSerializer.Serialize(ReadService.Query(request.File!,request.Method!,request.Arguments??[],request.Cases,request.Combinations)),
        _=>throw new ArgumentException("Unknown worker command.")
    });
    response=new(true,data,null);
}
catch(Exception ex){response=new(false,null,ex.GetBaseException().Message);}
File.WriteAllText(args[1],JsonSerializer.Serialize(response));return response.Success?0:1;
