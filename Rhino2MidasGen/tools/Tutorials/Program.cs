using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Runtime.InProcess;

internal static class Startup
{
    [STAThread] static int Main(string[] args)
    {
        RhinoInside.Resolver.Initialize(@"C:\Program Files\Rhino 8\System");
        return Host.Run(args);
    }
}
internal static class Host
{
    internal static readonly string[] Projects = ["Rhino2Straus", "Rhino2SAP", "Rhino2Midas", "Rhino2MidasGen"];
    internal static readonly Dictionary<string, Assembly> Assemblies = [];
    internal static string Root = "";
    internal static GH_ComponentServer Server = null!;
    [MethodImpl(MethodImplOptions.NoInlining)] internal static int Run(string[] args)
    {
        try
        {
            Root = Path.GetFullPath(args.Length > 0 ? args[0] : "..");
            using var rhino = new RhinoCore(["/nosplash"], WindowStyle.NoWindow);
            var folders = Projects.Select(p => Path.Combine(Root, p, "bin")).ToArray();
            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                var path = folders.Select(f => Path.Combine(f, name.Name + ".dll")).FirstOrDefault(File.Exists);
                return path == null ? null : context.LoadFromAssemblyPath(path);
            };
            Server = new GH_ComponentServer();
            typeof(Instances).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(f => f.FieldType == typeof(GH_ComponentServer)).SetValue(null, Server);
            var files = new List<GH_ExternalFile>();
            foreach (string name in new[] { "VectorComponents", "CurveComponents", "MeshComponents", "MathComponents", "SetsComponents" })
            {
                string path = Path.Combine(Grasshopper.Folders.PluginFolder, "Components", name + ".gha");
                if (File.Exists(path)) files.Add(new GH_ExternalFile(path));
            }
            foreach (var (project, folder) in Projects.Zip(folders))
            {
                string path = Path.Combine(folder, project + ".Grasshopper.gha");
                if (!File.Exists(path)) throw new FileNotFoundException("Run build.ps1 first.", path);
                Assemblies.Add(project, Assembly.LoadFrom(path));
                files.Add(new GH_ExternalFile(path));
            }
            var loader = typeof(GH_ComponentServer).GetMethod("LoadExternalFiles", BindingFlags.Instance | BindingFlags.NonPublic,
                null, [typeof(List<GH_ExternalFile>), typeof(bool)], null)!;
            if (!(bool)loader.Invoke(Server, [files, false])!) throw new Exception("Grasshopper registration failed.");
            GH_Document.EnableSolutions = true;
            if (args.Contains("--catalogue"))
            {
                string directory = Path.Combine(Root, "Rhino2MidasGen", ".local"); Directory.CreateDirectory(directory);
                foreach (var (project, assembly) in Assemblies)
                {
                    var catalogue = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(GH_Component).IsAssignableFrom(t))
                        .Select(t =>
                        {
                            var c = (GH_Component)Activator.CreateInstance(t)!;
                            return new { Class=t.Name, c.Name, c.ComponentGuid,
                                Inputs=c.Params.Input.Select(p => new { p.Name, Type=p.GetType().Name, Access=p.Access.ToString(), p.Optional, p.Description,
                                    Default=p.GetType().GetProperty("PersistentData")?.GetValue(p)?.ToString() }).ToArray(),
                                Outputs=c.Params.Output.Select(p => new {p.Name,Type=p.GetType().Name,Access=p.Access.ToString()}).ToArray() };
                        }).ToArray();
                    File.WriteAllText(Path.Combine(directory, project + "-tutorial-catalogue.json"), JsonSerializer.Serialize(catalogue, new JsonSerializerOptions{WriteIndented=true}));
                }
                Console.WriteLine("Component input/output catalogues written."); return 0;
            }
            Suite.Generate(args.Skip(1).FirstOrDefault());
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}