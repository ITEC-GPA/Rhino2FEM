using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Rhino2SAP.Core;

namespace Rhino2SAP.Api;

public interface ISapClient : IDisposable
{
    IReadOnlyDictionary<string,JsonElement> Call(string method,IReadOnlyDictionary<string,JsonElement>? arguments=null);
}

/// <summary>Calls the installed CSI interface; no proprietary assemblies are redistributed.</summary>
public sealed class SapClient : ISapClient
{
    public static Action<string>? Trace { get; set; }
    private readonly Assembly assembly;
    private readonly object application;
    private readonly object model;
    private readonly int threadId=Environment.CurrentManagedThreadId;
    private bool disposed;
    public static string FindInstallation(string? configured=null)
    {
        var candidate=configured??Environment.GetEnvironmentVariable("SAP2000_PATH");
        if(!string.IsNullOrWhiteSpace(candidate))
        {
            if(File.Exists(candidate))candidate=Path.GetDirectoryName(candidate);
            if(File.Exists(Path.Combine(candidate!,"SAP2000.exe")))return Path.GetFullPath(candidate!);
            throw new DirectoryNotFoundException("SAP2000_PATH must identify the SAP2000 installation or executable.");
        }
        var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Computers and Structures");
        var found=Directory.Exists(root)?Directory.GetDirectories(root,"SAP2000 *").OrderByDescending(p=>int.TryParse(Path.GetFileName(p).Split(' ').Last(),out var v)?v:0).FirstOrDefault(p=>File.Exists(Path.Combine(p,"SAP2000v1.dll"))):null;
        return found??throw new FileNotFoundException("Install SAP2000 26 or set SAP2000_PATH.");
    }
    public SapClient(string? installation=null,bool visible=false)
    {
        if(Thread.CurrentThread.GetApartmentState()!=ApartmentState.STA)throw new InvalidOperationException("SAP API calls require an STA thread.");
        string folder=FindInstallation(installation);
        Trace?.Invoke("Loading SAP SDK: "+folder);
        assembly=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(folder,"SAP2000v1.dll"));
        object helper=Activator.CreateInstance(assembly.GetType("SAP2000v1.Helper",true)!)!;
        Trace?.Invoke("Creating isolated SAP2000 object");
        application=Invoke(assembly.GetType("SAP2000v1.cHelper",true)!,helper,"CreateObject",[Path.Combine(folder,"SAP2000.exe")])!;
        try
        {
            var t=assembly.GetType("SAP2000v1.cOAPI",true)!;
            var start=t.GetMethod("ApplicationStart")!;
            var args=start.GetParameters().Select(p=>p.HasDefaultValue?p.DefaultValue:Type.Missing).ToArray();
            for(int i=0;i<args.Length;i++)if(string.Equals(start.GetParameters()[i].Name,"visible",StringComparison.OrdinalIgnoreCase))args[i]=visible;
            Trace?.Invoke("Calling ApplicationStart (hidden)");
            Check("ApplicationStart",start.Invoke(application,args));
            Trace?.Invoke("SAP2000 API ready");
            model=t.GetProperty("SapModel")!.GetValue(application)!;
        }
        catch { try{Invoke(assembly.GetType("SAP2000v1.cOAPI",true)!,application,"ApplicationExit",[false]);}catch{} throw; }
    }
    public IReadOnlyDictionary<string,JsonElement> Call(string key,IReadOnlyDictionary<string,JsonElement>? arguments=null)
    {
        if(disposed)throw new ObjectDisposedException(nameof(SapClient));
        if(threadId!=Environment.CurrentManagedThreadId)throw new InvalidOperationException("SAP API calls must stay on the owning thread.");
        var spec=ApiSchema.Get(key);var target=model;var type=assembly.GetType("SAP2000v1.cSapModel",true)!;
        foreach(var segment in spec.Path.Split('.',StringSplitOptions.RemoveEmptyEntries))
        { var property=type.GetProperty(segment)!;target=property.GetValue(target)!;type=property.PropertyType; }
        var method=type.GetMethod(spec.Method)??throw new MissingMethodException(type.FullName,spec.Method);
        var parameters=method.GetParameters();var values=new object?[parameters.Length];
        for(int i=0;i<parameters.Length;i++)
        {
            var p=parameters[i];var t=p.ParameterType.IsByRef?p.ParameterType.GetElementType()!:p.ParameterType;
            if(arguments!=null&&arguments.TryGetValue(p.Name!,out var value))values[i]=ConvertValue(value,t);
            else if(p.HasDefaultValue)values[i]=p.DefaultValue;
            else if(p.ParameterType.IsByRef)values[i]=t.IsArray?Array.CreateInstance(t.GetElementType()!,0):t==typeof(string)?"":Activator.CreateInstance(t);
            else throw new ArgumentException($"{key}: missing {p.Name}.");
        }
        try { Check(key,method.Invoke(target,values)); }
        catch(TargetInvocationException ex){throw new InvalidOperationException($"SAP {key}: {ex.InnerException?.Message}",ex.InnerException);}
        return parameters.Select((p,i)=>(p,i)).Where(x=>x.p.ParameterType.IsByRef).ToDictionary(x=>x.p.Name!,x=>ApiSchema.Value(values[x.i]));
    }
    private static object? ConvertValue(JsonElement value,Type t)
    {
        if(t.IsArray){var items=value.EnumerateArray().ToArray();var a=Array.CreateInstance(t.GetElementType()!,items.Length);for(int i=0;i<items.Length;i++)a.SetValue(ConvertValue(items[i],t.GetElementType()!),i);return a;}
        if(t.IsEnum)return Enum.ToObject(t,value.GetInt32());
        return JsonSerializer.Deserialize(value.GetRawText(),t);
    }
    private static object? Invoke(Type type,object obj,string method,object?[] values)=>type.GetMethod(method)!.Invoke(obj,values);
    private static void Check(string key,object? result){if(result is int code&&code!=0)throw new InvalidOperationException($"SAP {key} failed (return code {code}).");}
    public void Dispose()
    {
        if(disposed)return;
        if(threadId!=Environment.CurrentManagedThreadId)throw new InvalidOperationException("Dispose SAP on the owning STA thread.");
        try{Check("ApplicationExit",Invoke(assembly.GetType("SAP2000v1.cOAPI",true)!,application,"ApplicationExit",[false]));}finally{disposed=true;}
    }
}

public static class SapThread
{
    private static readonly object Gate=new();
    public static T Run<T>(Func<T> action)
    {
        lock(Gate)
        {
            T result=default!;Exception? error=null;
            var thread=new Thread(()=>{try{result=action();}catch(Exception ex){error=ex;}}){IsBackground=true,Name="Rhino2SAP API"};
            thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();
            if(error!=null)System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
            return result;
        }
    }
}
