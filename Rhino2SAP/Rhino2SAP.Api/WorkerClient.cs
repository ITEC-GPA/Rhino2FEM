using System.Diagnostics;
using System.Text.Json;
using Rhino2SAP.Core;

namespace Rhino2SAP.Api;

public sealed record WorkerRequest(string Command,string? ModelJson=null,RunOptions? Options=null,string? File=null,string? Method=null,Dictionary<string,JsonElement>? Arguments=null,string[]? Cases=null,string[]? Combinations=null);
public sealed record WorkerResponse(bool Success,string? Data,string? Error);

/// <summary>Owns a disposable helper process, bounding COM startup hangs outside the Rhino process.</summary>
public static class WorkerClient
{
    private static readonly object Gate=new();
    public static SapModel Execute(SapModel model,RunOptions options)=>ModelArchive.Deserialize(Invoke(new("Run",ModelArchive.Serialize(model),options)));
    public static Fragment Geometry(string file)=>ModelArchive.Deserialize(Invoke(new("Geometry",File:file))).Definition;
    public static SapModel ImportModel(string file)=>ModelArchive.Deserialize(Invoke(new("ImportModel",File:file)));
    public static ResultTable Query(string file,string method,Dictionary<string,JsonElement> arguments,string[] cases,string[] combinations)=>JsonSerializer.Deserialize<ResultTable>(Invoke(new("Query",File:file,Method:method,Arguments:arguments,Cases:cases,Combinations:combinations)))!;
    public static string Invoke(WorkerRequest request,int startupTimeoutSeconds=120,int analysisTimeoutSeconds=1800)
    {
        lock(Gate)
        {
            string folder=Path.GetDirectoryName(typeof(WorkerClient).Assembly.Location)!;
            string worker=Path.Combine(folder,"Rhino2SAP.Worker.dll");
            if(!File.Exists(worker))throw new FileNotFoundException("Install the complete Rhino2SAP package, including the API worker.",worker);
            string temp=Path.Combine(Path.GetTempPath(),"Rhino2SAP-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
            string input=Path.Combine(temp,"request.json"),output=Path.Combine(temp,"response.json");
            try
            {
                File.WriteAllText(input,JsonSerializer.Serialize(request));
                var start=new ProcessStartInfo("dotnet"){CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=folder};
                start.ArgumentList.Add(worker);start.ArgumentList.Add(input);start.ArgumentList.Add(output);
                using var process=new Process{StartInfo=start};int ready=0;var log=new System.Collections.Concurrent.ConcurrentQueue<string>();
                process.OutputDataReceived+=(_,e)=>{if(e.Data!=null){log.Enqueue(e.Data);if(e.Data.Contains("SAP2000 API ready",StringComparison.Ordinal))Interlocked.Exchange(ref ready,1);}};
                process.ErrorDataReceived+=(_,e)=>{if(e.Data!=null)log.Enqueue(e.Data);};
                process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();var timer=Stopwatch.StartNew();bool enteredAnalysis=false;
                while(!process.WaitForExit(200))
                {
                    if(Volatile.Read(ref ready)==1&&!enteredAnalysis){enteredAnalysis=true;timer.Restart();}
                    if(timer.Elapsed.TotalSeconds>(enteredAnalysis?analysisTimeoutSeconds:startupTimeoutSeconds))
                    {
                        process.Kill(entireProcessTree:true);process.WaitForExit();
                        throw new TimeoutException(enteredAnalysis?"SAP analysis exceeded the time limit; its helper process was stopped.":"SAP2000 did not complete ApplicationStart within the startup timeout. The isolated helper was stopped. Check the installed SAP application and its API/license availability.");
                    }
                }
                process.WaitForExit();
                if(!File.Exists(output))throw new InvalidOperationException("SAP worker exited without a response: "+string.Join("\n",log.TakeLast(8)));
                var response=JsonSerializer.Deserialize<WorkerResponse>(File.ReadAllText(output))!;
                if(!response.Success)throw new InvalidOperationException(response.Error);
                return response.Data!;
            }
            finally { Directory.Delete(temp,true); }
        }
    }
}
