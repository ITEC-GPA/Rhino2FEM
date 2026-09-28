using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Runtime.InProcess;

internal static class Startup
{
    [STAThread] static int Main(string[] args)
    {
        RhinoInside.Resolver.Initialize(@"C:\Program Files\Rhino 8\System");
        return Program.Run(args);
    }
}
internal static class Program
{
    static readonly string[] Projects = ["Rhino2Straus", "Rhino2SAP", "Rhino2Midas", "Rhino2MidasGen"];
    internal static readonly Dictionary<string, Assembly> Assemblies = [];
    internal static GH_ComponentServer Server = null!;
    internal static string Root = "";
    [MethodImpl(MethodImplOptions.NoInlining)] internal static int Run(string[] args)
    {
        try
        {
            Root = Path.GetFullPath(args.Length > 0 ? args[0] : "..");
            using var rhino = new RhinoCore(["/nosplash"], WindowStyle.NoWindow);
            var folders = Projects.Select(p => Path.Combine(Root, p, p + ".Grasshopper", "bin", "Release", "net8.0-windows")).ToArray();
            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                var path = folders.Select(f => Path.Combine(f, name.Name + ".dll")).FirstOrDefault(File.Exists);
                return path == null ? null : context.LoadFromAssemblyPath(path);
            };
            // Isolate this test host from unrelated, installed Grasshopper add-ons. These
            // Rhino 8 internal entry points are used only by this generator, never by a .gh file.
            Server = new GH_ComponentServer();
            typeof(Instances).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(f => f.FieldType == typeof(GH_ComponentServer)).SetValue(null, Server);
            var files = new List<GH_ExternalFile>();
            foreach (string name in new[] { "VectorComponents", "CurveComponents" })
                files.Add(new GH_ExternalFile(Path.Combine(Grasshopper.Folders.PluginFolder, "Components", name + ".gha")));
            foreach (var (project, folder) in Projects.Zip(folders))
            {
                string path = Path.Combine(folder, project + ".Grasshopper.gha");
                if (!File.Exists(path)) throw new FileNotFoundException("Build all four plugins in Release first.", path);
                Assemblies.Add(project, Assembly.LoadFrom(path));
                files.Add(new GH_ExternalFile(path));
            }
            var loader = typeof(GH_ComponentServer).GetMethod("LoadExternalFiles", BindingFlags.Instance | BindingFlags.NonPublic,
                null, [typeof(List<GH_ExternalFile>), typeof(bool)], null) ?? throw new NotSupportedException("Unsupported Rhino/Grasshopper SDK.");
            if (!(bool)loader.Invoke(Server, [files, false])!) throw new Exception("Grasshopper component registration failed.");
            GH_Document.EnableSolutions = true;
            Straus(); Sap(); Civil(); Gen();
            Console.WriteLine("PASS: all four .gh and .ghx definitions reopened, solved and checked.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    static void Straus()
    {
        using var b = new Example("Rhino2Straus", "FEM_01_Straus_Mensola", "01 / STRAUS7 - Mensola parametrica",
            "Modifica L, b, h e Fz. Coordinate in metri, forze in kN. Segui i gruppi da sinistra a destra.\nIl Model contiene una mensola incastrata e il carico LC1; la preview funziona senza aprire Straus7.");
        var length = b.Slider("L [m]", 3, 1, 10, 60, 210);
        var height = b.Slider("h [m]", .3, .1, .8, 60, 340);
        var width = b.Slider("b [m]", .2, .1, .6, 60, 405);
        var force = b.Slider("Fz [kN]", -10, -100, 0, 60, 710);
        var fixedDofs = b.Toggle("Incastro: 6 DOF", true, 840, 635);
        var origin = b.Point("A", null, null, 420, 180);
        var end = b.Point("B", length, null, 420, 270);
        var line = b.Line(origin, end, 840, 190);
        var material = b.C("MaterialComponent", 420, 430);
        var section = b.C("FrameSectionRectangularComponent", 420, 610); b.Set(section, "Name", "R200x300");
        b.Wire(height, section, "Height"); b.Wire(width, section, "Width");
        var property = b.C("FramePropertyComponent", 840, 435); b.Set(property, "Name", "S1");
        b.Wire(section, property, "Section"); b.Wire(material, property, "Material");
        var beam = b.C("FrameElementComponent", 1250, 250); b.Set(beam, "Frame ID", 1);
        b.Wire(line, beam, "Frame line"); b.Wire(property, beam, "Frame Property");
        var nodeA = b.C("NodeElementComponent", 840, 755); b.Set(nodeA, "Node ID", 1); b.Wire(origin, nodeA, "Node point");
        var nodeB = b.C("NodeElementComponent", 840, 935); b.Set(nodeB, "Node ID", 2); b.Wire(end, nodeB, "Node point");
        var support = b.C("NodeSupportComponent", 1250, 710); b.Wire(nodeA, support, "Node");
        foreach (string name in new[] { "Dx", "Dy", "Dz", "Mx", "My", "Mz" }) b.Wire(fixedDofs, support, name);
        var lc = b.C("LoadCaseComponent", 420, 920); b.Set(lc, "Name", "LC1");
        var load = b.C("LoadNodalComponent", 1250, 980); b.Wire(nodeB, load, "Node"); b.Wire(lc, load, "Load case"); b.Wire(force, load, "Fz");
        var units = b.C("ModelUnitsComponent", 1680, 300);
        var model = b.C("BuildModelComponent", 1680, 620);
        b.Wire(units, model, "Unit"); b.Wire(beam, model, "Frame element"); b.Wire(support, model, "Node"); b.Wire(load, model, "Node");
        b.Results(model, 2, "Frame elements", 1, .18, true);
        b.Group("01  PARAMETRI", Color.LightGoldenrodYellow, length, height, width, force);
        b.Group("02  GEOMETRIA E PROPRIETA", Color.LightBlue, origin, end, line, material, section, property, beam);
        b.Group("03  VINCOLI E CARICO", Color.MistyRose, fixedDofs, nodeA, nodeB, support, lc, load);
        b.Group("04  MODELLO", Color.Honeydew, units, model);
        b.Save();
    }

    static void Civil()
    {
        using var b = new Example("Rhino2Midas", "FEM_03_Civil_Trave", "03 / CIVIL NX - Trave, peso proprio e combinazione",
            "Modifica L, b, h e q. Unita: kN / m. G = peso proprio; Q = carico verticale distribuito.\nLa combinazione ULS = 1.35 G + 1.50 Q e inclusa nel Model. Nessuna connessione API serve per questa definizione.");
        var length = b.Slider("L [m]", 6, 2, 15, 60, 200);
        var height = b.Slider("h [m]", .5, .2, 1.2, 60, 350);
        var width = b.Slider("b [m]", .3, .15, .8, 60, 415);
        var q = b.Slider("q GZ [kN/m]", -8, -40, 0, 60, 690);
        var origin = b.Point("A", null, null, 420, 180); var end = b.Point("B", length, null, 420, 280);
        var mat = b.C("MaterialComponent", 420, 470); b.Concrete(mat);
        var section = b.C("RectangleSectionComponent", 840, 460); b.Set(section, "Name", "R300x500"); b.Wire(height, section, "Height"); b.Wire(width, section, "Width");
        var nodes = b.C("NodeElementComponent", 840, 205); b.Wire(origin, nodes, "Point"); b.Wire(end, nodes, "Point"); b.Set(nodes, "ID", 1, 2);
        var beam = b.C("BeamElementComponent", 1250, 300); b.Wire(origin, beam, "Points"); b.Wire(end, beam, "Points");
        b.Wire(nodes, beam, "Nodes"); b.Wire(mat, beam, "Material"); b.Wire(section, beam, "Property");
        var baseNode = b.C("NodeElementComponent", 840, 655); b.Wire(origin, baseNode, "Point");
        var support = b.C("SupportComponent", 1250, 560); b.Wire(baseNode, support, "Nodes");
        var g = b.C("LoadCaseComponent", 420, 780); b.Set(g, "Name", "G"); b.Set(g, "Type", "D");
        var live = b.C("LoadCaseComponent", 420, 950); b.Set(live, "ID", 2); b.Set(live, "Name", "Q"); b.Set(live, "Type", "L");
        var sw = b.C("SelfWeightComponent", 1250, 740); b.Wire(g, sw, "Case", "Name");
        var dl = b.C("BeamDistributedLoadComponent", 1250, 965); b.Wire(beam, dl, "Beam"); b.Wire(live, dl, "Case", "Name"); b.Wire(q, dl, "Start value"); b.Wire(q, dl, "End value");
        var combo = b.C("LoadCombinationComponent", 1680, 1030); b.Set(combo, "Name", "ULS");
        b.Wire(g, combo, "Cases", "Name"); b.Wire(live, combo, "Cases", "Name"); b.Set(combo, "Factors", 1.35, 1.5);
        var units = b.C("UnitsComponent", 1680, 250); var model = b.C("BuildModelComponent", 1680, 610);
        b.Wire(units, model, "Units"); foreach (var c in new[] { dl, support, g, live, sw, combo }) b.Wire(c, model, "Definitions");
        b.Results(model, 2, "Beam", 1, .9);
        b.Group("01  PARAMETRI", Color.LightGoldenrodYellow, length, height, width, q);
        b.Group("02  GEOMETRIA E PROPRIETA", Color.LightBlue, origin, end, nodes, mat, section, beam);
        b.Group("03  VINCOLI E CARICHI", Color.MistyRose, baseNode, support, g, live, sw, dl);
        b.Group("04  MODELLO E COMBINAZIONE", Color.Honeydew, units, model, combo);
        b.Save();
    }

    static void Gen()
    {
        using var b = new Example("Rhino2MidasGen", "FEM_04_GEN_Parete", "04 / GEN NX - Parete, piani e diaframma",
            "Parete 4 x 3 m, spessore 0.20 m, base incastrata. Carico FX = 5 kN per nodo superiore: totale 10 kN.\nModifica larghezza, altezza e spessore. Il piano Roof segue h; il toggle abilita il diaframma del piano.");
        var width = b.Slider("L [m]", 4, 1, 10, 60, 220);
        var height = b.Slider("h [m]", 3, 1, 6, 60, 290);
        var thickness = b.Slider("t [m]", .2, .1, .6, 60, 460);
        var diaphragm = b.Toggle("Diaframma Roof", true, 60, 630);
        var p0 = b.Point("A", null, null, 420, 160); var p1 = b.Point("B", width, null, 420, 255);
        var p2 = b.Point("C", width, height, 420, 350); var p3 = b.Point("D", null, height, 420, 445);
        var baseNodes = b.C("NodeElementComponent", 840, 210); b.Wire(p0, baseNodes, "Point"); b.Wire(p1, baseNodes, "Point"); b.Set(baseNodes, "ID", 1, 2);
        var topNodes = b.C("NodeElementComponent", 840, 400); b.Wire(p2, topNodes, "Point"); b.Wire(p3, topNodes, "Point"); b.Set(topNodes, "ID", 3, 4);
        var mat = b.C("MaterialComponent", 420, 690); b.Concrete(mat);
        var prop = b.C("ThicknessComponent", 840, 655); b.Wire(thickness, prop, "Thickness");
        var wall = b.C("WallElementComponent", 1250, 310);
        foreach (var p in new[] { p0, p1, p2, p3 }) b.Wire(p, wall, "Points");
        b.Wire(baseNodes, wall, "Nodes"); b.Wire(topNodes, wall, "Nodes"); b.Wire(mat, wall, "Material"); b.Wire(prop, wall, "Property");
        var support = b.C("SupportComponent", 1250, 585); b.Wire(baseNodes, support, "Nodes");
        var lc = b.C("LoadCaseComponent", 840, 855); b.Set(lc, "Name", "WIND");
        var load = b.C("NodalLoadComponent", 1250, 865); b.Wire(topNodes, load, "Nodes"); b.Wire(lc, load, "Case", "Name"); b.Set(load, "Loads", 5d, 0d, 0d, 0d, 0d, 0d);
        var story0 = b.C("StoryComponent", 840, 1080); b.Set(story0, "Name", "Base"); b.Set(story0, "Level", 0d);
        var story1 = b.C("StoryComponent", 1250, 1100); b.Set(story1, "ID", 2); b.Set(story1, "Name", "Roof"); b.Wire(height, story1, "Level"); b.Wire(diaphragm, story1, "Rigid diaphragm");
        var units = b.C("UnitsComponent", 1680, 280); var model = b.C("BuildModelComponent", 1680, 635);
        b.Wire(units, model, "Units"); foreach (var c in new[] { wall, support, load, lc, story0, story1 }) b.Wire(c, model, "Definitions");
        b.Results(model, 4, "Wall", 1, 2.4);
        b.Group("01  PARAMETRI", Color.LightGoldenrodYellow, width, height, thickness, diaphragm);
        b.Group("02  GEOMETRIA E PROPRIETA", Color.LightBlue, p0, p1, p2, p3, baseNodes, topNodes, mat, prop, wall);
        b.Group("03  VINCOLI, CARICO E PIANI", Color.MistyRose, support, lc, load, story0, story1);
        b.Group("04  MODELLO", Color.Honeydew, units, model);
        b.Save();
    }

    static void Sap()
    {
        using var b = new Example("Rhino2SAP", "FEM_02_SAP_Portale", "02 / SAP2000 - Portale parametrico",
            "Portale nel piano XZ: L = 6 m, H = 3 m, sezione 0.25 x 0.35 m, basi incastrate. Unita: kN / m.\nG = peso proprio; Q = carico GZ sulla sola trave. ULS = 1.35 G + 1.50 Q. I comandi API restano nel Model.");
        var length = b.Slider("L [m]", 6, 2, 12, 60, 180); var height = b.Slider("H [m]", 3, 2, 6, 60, 250);
        var depth = b.Slider("h sezione [m]", .35, .2, .8, 60, 430); var width = b.Slider("b sezione [m]", .25, .15, .6, 60, 500);
        var q = b.Slider("q GZ [kN/m]", -8, -40, 0, 60, 745);
        var p0 = b.Point("A", null, null, 420, 170); var p1 = b.Point("B", null, height, 420, 265);
        var p2 = b.Point("C", length, height, 420, 360); var p3 = b.Point("D", length, null, 420, 455);
        var l0 = b.Line(p0, p1, 840, 175); var l1 = b.Line(p1, p2, 840, 285); var l2 = b.Line(p2, p3, 840, 395);
        var mat = b.C("MaterialComponent", 420, 700); b.Set(mat, "Name", "C30"); b.Set(mat, "Type", 2); b.Set(mat, "E", 30e6); b.Set(mat, "Poisson", .2); b.Set(mat, "Weight density", 25d);
        var section = b.C("PropFrame_SetRectangleComponent", 840, 650); b.Set(section, "Name", "R250x350"); b.Wire(mat, section, "MatProp"); b.Wire(depth, section, "T3"); b.Wire(width, section, "T2");
        var frames = b.C("FrameElementComponent", 1250, 280); b.Set(frames, "Name", "ColL", "Beam", "ColR");
        foreach (var line in new[] { l0, l1, l2 }) b.Wire(line, frames, "Line"); b.Wire(section, frames, "Property");
        var nodes = b.C("NodeElementComponent", 840, 885); b.Set(nodes, "Name", "N1", "N2"); b.Wire(p0, nodes, "Point"); b.Wire(p3, nodes, "Point");
        var support = b.C("PointObj_SetRestraintComponent", 1250, 590); b.Wire(nodes, support, "Name"); b.Set(support, "Value", true, true, true, true, true, true);
        var g = b.C("LoadPatternComponent", 420, 945); b.Set(g, "Name", "G"); b.Set(g, "Type", 1); b.Set(g, "Self weight", 1d);
        var live = b.C("LoadPatternComponent", 420, 1110); b.Set(live, "Name", "Q"); b.Set(live, "Type", 3);
        var dl = b.C("FrameObj_SetLoadDistributedComponent", 1250, 915); b.Wire(frames, dl, "Definitions"); b.Set(dl, "Name", "Beam"); b.Wire(live, dl, "LoadPat");
        b.Set(dl, "MyType", 1); b.Set(dl, "Dir", 6); b.Set(dl, "Dist1", 0d); b.Set(dl, "Dist2", 1d); b.Wire(q, dl, "Val1"); b.Wire(q, dl, "Val2");
        var combo = b.C("CombinationComponent", 1680, 1060); b.Set(combo, "Name", "ULS"); b.Set(combo, "Cases", "G", "Q"); b.Set(combo, "Factors", 1.35, 1.5);
        var units = b.SapUnits(1680, 290); var model = b.C("BuildModelComponent", 1680, 635);
        b.Wire(units, model, "Units"); foreach (var c in new[] { dl, support, g, combo }) b.Wire(c, model, "Definitions");
        b.Results(model, 4, "Frame", 3, 1.05);
        b.Group("01  PARAMETRI", Color.LightGoldenrodYellow, length, height, depth, width, q);
        b.Group("02  GEOMETRIA E PROPRIETA", Color.LightBlue, p0, p1, p2, p3, l0, l1, l2, mat, section, frames);
        b.Group("03  VINCOLI E CARICHI", Color.MistyRose, nodes, support, g, live, dl);
        b.Group("04  MODELLO E COMBINAZIONE", Color.Honeydew, units, model, combo);
        b.Save();
    }
}

internal sealed class Example : IDisposable
{
    readonly string project, name;
    readonly GH_Document doc = new();
    readonly List<GH_Component> components = [];
    GH_Component validation = null!, decompose = null!, physical = null!;
    int nodeCount, elementCount; string elementPort = ""; double volume;
    readonly List<object> connections = [];
    public Example(string project, string name, string title, string explanation)
    {
        this.project = project; this.name = name;
        doc.Properties.Description = explanation; doc.Properties.ProjectFileName = name + ".gh";
        doc.Properties.ZoomFactor = .5f; doc.Properties.ViewTarget = new System.Drawing.Point(40, 70);
        Panel(title, explanation, 20, -60, 2790, 115);
    }
    T Add<T>(T obj, float x, float y) where T : IGH_DocumentObject
    {
        obj.CreateAttributes(); obj.Attributes.Pivot = new PointF(x, y); doc.AddObject(obj, false);
        if (obj is GH_Component c) components.Add(c);
        return obj;
    }
    public GH_Component C(string className, float x, float y)
    {
        var type = Program.Assemblies[project].GetTypes().Single(t => t.Name == className && !t.IsAbstract);
        var c = (GH_Component)Activator.CreateInstance(type)!;
        // Draw only the final display manager; intermediate geometry remains available via wires.
        c.Hidden = true;
        return Add(c, x, y);
    }
    GH_Component Native(string guid, float x, float y, string nick)
    {
        var c = (GH_Component)(Program.Server.EmitObject(Guid.Parse(guid)) ?? throw new Exception("Native GH component missing."));
        c.NickName = nick; c.Hidden = true; return Add(c, x, y);
    }
    public GH_Component Point(string nick, IGH_DocumentObject? x, IGH_DocumentObject? z, float px, float py)
    {
        var c = Native("3581f42a-9592-4549-bd6b-1c0fc39d067b", px, py, nick);
        if (x != null) Wire(x, c, "X coordinate"); if (z != null) Wire(z, c, "Z coordinate"); return c;
    }
    public GH_Component Line(IGH_DocumentObject a, IGH_DocumentObject b, float x, float y)
    {
        var c = Native("4c4e56eb-2f04-43f9-95a3-cc46a14f495a", x, y, "Asse"); Wire(a, c, "Start Point"); Wire(b, c, "End Point"); return c;
    }
    public GH_NumberSlider Slider(string name, double value, double min, double max, float x, float y)
    {
        var s = Add(new GH_NumberSlider { NickName = name }, x, y);
        s.Slider.Minimum = (decimal)min; s.Slider.Maximum = (decimal)max; s.Slider.DecimalPlaces = 2; s.SetSliderValue((decimal)value);
        s.Attributes.Bounds = new RectangleF(x, y, 250, 30); return s;
    }
    public GH_ValueList SapUnits(float x, float y)
    {
        var list = Add(new GH_ValueList { NickName = "Unita: kN / m / C" }, x, y);
        list.ListItems.Clear(); list.ListItems.Add(new GH_ValueListItem("kN_m_C", "6") { Selected = true }); return list;
    }
    public GH_BooleanToggle Toggle(string name, bool value, float x, float y) => Add(new GH_BooleanToggle { NickName = name, Value = value }, x, y);
    public GH_Panel Panel(string nick, string text, float x, float y, float width = 280, float height = 170)
    {
        var p = Add(new GH_Panel { NickName = nick, UserText = text }, x, y); p.Attributes.Bounds = new RectangleF(x, y, width, height); return p;
    }
    static IGH_Param Out(IGH_DocumentObject obj, string? port = null) => obj is GH_Component c ? port == null ? c.Params.Output[0] : Find(c.Params.Output, port) : (IGH_Param)obj;
    static IGH_Param Find(IEnumerable<IGH_Param> ports, string name) => ports.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException("Missing port '" + name + "'; available: " + string.Join(", ", ports.Select(p => p.Name)));
    public void Wire(IGH_DocumentObject from, GH_Component to, string input, string? output = null)
    {
        var source = Out(from, output); var target = Find(to.Params.Input, input); target.AddSource(source);
        connections.Add(new { FromId = from.InstanceGuid, From = from.NickName, Component = from.Name, Output = source.Name, ToId = to.InstanceGuid, To = to.Name, Input = target.Name });
    }
    public void Set(GH_Component c, string input, params object[] values)
    {
        var p = Find(c.Params.Input, input);
        switch (p)
        {
            case Param_Number n: n.PersistentData.Clear(); foreach (var v in values) n.PersistentData.Append(new GH_Number(Convert.ToDouble(v))); break;
            case Param_Integer n: n.PersistentData.Clear(); foreach (var v in values) n.PersistentData.Append(new GH_Integer(Convert.ToInt32(v))); break;
            case Param_String s: s.PersistentData.Clear(); foreach (var v in values) s.PersistentData.Append(new GH_String((string)v)); break;
            case Param_Boolean b: b.PersistentData.Clear(); foreach (var v in values) b.PersistentData.Append(new GH_Boolean((bool)v)); break;
            case Param_GenericObject g: g.PersistentData.Clear(); foreach (var v in values) g.PersistentData.Append(v switch { string s => new GH_String(s), int n => new GH_Integer(n), double n => new GH_Number(n), bool b => new GH_Boolean(b), _ => throw new ArgumentException("Unsupported example value") }); break;
            default: throw new ArgumentException("Unsupported parameter: " + p.GetType().Name);
        }
    }
    public void Concrete(GH_Component mat)
    { Set(mat, "Name", "C30"); Set(mat, "E", 30e6); Set(mat, "Poisson", .2); Set(mat, "Weight density", 25d); Set(mat, "Type", "CONC"); }
    public void Group(string title, Color colour, params IGH_DocumentObject[] objects)
    {
        var group = new GH_Group { NickName = title, Colour = Color.FromArgb(65, colour) };
        foreach (var obj in objects) group.AddObject(obj.InstanceGuid); Add(group, 0, 0);
    }
    public void Results(GH_Component model, int nodes, string elementPort, int elements, double expectedVolume, bool straus = false)
    {
        nodeCount = nodes; elementCount = elements; this.elementPort = elementPort; volume = expectedVolume;
        validation = C("ValidateModelComponent", 2110, 210); Wire(model, validation, "Model");
        var validPanel = Panel("VALIDAZIONE", "", 2500, 130, 270, 90); validPanel.AddSource(validation.Params.Output[0]);
        var summary = C("ModelSummaryComponent", 2110, 430); Wire(model, summary, "Model");
        var summaryPanel = Panel("RIEPILOGO MODELLO", "", 2500, 295, 270, 250); summaryPanel.AddSource(summary.Params.Output[0]);
        var display = C("DisplaySettingsComponent", 2110, 715);
        var preview = C("PreviewModelComponent", 2500, 730); preview.Hidden = false; Wire(model, preview, "Model"); Wire(display, preview, "Settings");
        physical = C(straus ? "ModelGeometryComponent" : "PhysicalGeometryComponent", 2110, 1030); Wire(model, physical, "Model");
        decompose = C("DecomposeModelComponent", 2110, 1330); Wire(model, decompose, "Model");
        var info = C(straus ? "BeamInfoComponent" : elementPort == "Wall" ? "DecomposeWallComponent" : "DecomposeBeamComponent", 2500, 1280);
        Wire(decompose, info, straus ? "Beam" : "Element", elementPort);
        Group("05  CONTROLLI, PREVIEW E GEOMETRIA", Color.Lavender, validation, validPanel, summary, summaryPanel, display, preview, physical, decompose, info);
    }
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    void Check(GH_Document document, double? changedVolume = null)
    {
        document.Enabled = true; document.NewSolution(false, GH_SolutionMode.Silent);
        foreach (var c in document.Objects.OfType<GH_Component>())
        {
            var errors = c.RuntimeMessages(GH_RuntimeMessageLevel.Error);
            Require(errors.Count == 0, c.Name + ": " + string.Join("; ", errors));
            Require(c.Params.Output.Sum(p => p.VolatileDataCount) > 0, c.Name + " produced no outputs.");
        }
        GH_Component Match(GH_Component original) => document.Objects.OfType<GH_Component>().Single(c => c.InstanceGuid == original.InstanceGuid);
        var flags = Match(validation).Params.Output[0].VolatileData.AllData(true).OfType<GH_Boolean>().ToArray();
        Require(flags.Length == 1 && flags[0].Value, "Invalid or multiple example Models: " + flags.Length + "; " + string.Join("; ", document.Objects.OfType<GH_Component>().Where(c => c.Name.Contains("Build")).Select(c => c.Name + "=" + c.Params.Output[0].VolatileDataCount + ": " + string.Join("; ", c.Params.Input.Select(p => p.Name + "=" + p.VolatileDataCount + " paths=" + string.Join(",", p.VolatileData.Paths))))));
        var decomp = Match(decompose);
        Require(decomp.Params.Output[0].VolatileDataCount == nodeCount, "Unexpected node count: " + decomp.Params.Output[0].VolatileDataCount);
        Require(Find(decomp.Params.Output, elementPort).VolatileDataCount == elementCount, "Unexpected element count.");
        var physicalPorts = Match(physical).Params.Output;
        var brepPort = physicalPorts.LastOrDefault(p => p is Param_Brep) ?? physicalPorts[0];
        var breps = brepPort.VolatileData.AllData(true).Select(g => g.ScriptVariable()).OfType<Brep>().ToArray();
        Require(breps.Length == elementCount && breps.All(b => b.IsSolid && b.IsValid), "Invalid physical geometry.");
        double actualVolume = breps.Sum(b => VolumeMassProperties.Compute(b).Volume);
        Require(Math.Abs(actualVolume - (changedVolume ?? volume)) < 1e-7, $"Unexpected physical volume {actualVolume}; expected {changedVolume ?? volume}.");
    }
    public void Save()
    {
        Check(doc);
        var slider = doc.Objects.OfType<GH_NumberSlider>().First(); decimal initial = slider.CurrentValue;
        slider.SetSliderValue(initial * 1.1m); slider.ExpireSolution(false);
        double changedVolume = project == "Rhino2SAP" ? volume + (double)(initial * .1m) * .35 * .25 : volume * 1.1;
        Check(doc, changedVolume);
        slider.SetSliderValue(initial); slider.ExpireSolution(false); Check(doc);
        string folder = Path.Combine(Program.Root, project, "examples"); Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, name);
        foreach (string ext in new[] { ".gh", ".ghx" })
        {
            Require(new GH_DocumentIO(doc).SaveQuiet(path + ext), "Cannot save " + ext);
            var io = new GH_DocumentIO(); Require(io.Open(path + ext), "Cannot reopen " + ext);
            using var reopened = io.Document;
            Require(reopened.Objects.Count == doc.Objects.Count, "Objects lost during serialization.");
            int Wires(GH_Document d) => d.Objects.OfType<GH_Component>().Sum(c => c.Params.Input.Sum(p => p.SourceCount)) + d.Objects.OfType<IGH_Param>().Sum(p => p.SourceCount);
            Require(Wires(reopened) == Wires(doc), "Connections lost during serialization."); Check(reopened);
        }

        File.WriteAllText(path + ".connections.json", JsonSerializer.Serialize(new { Plugin = project, File = name + ".gh", PluginComponents = components.Count(c => c.GetType().Assembly == Program.Assemblies[project]), Objects = doc.Objects.Count, Nodes = nodeCount, Elements = elementCount, PhysicalVolume = volume, RoundTripGh = true, RoundTripGhx = true, ParametricLengthChange = true, SolverExecuted = false, Connections = connections }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"PASS {project}/{name}: {components.Count} components, {nodeCount} nodes, {elementCount} elements, {volume} m3; .gh/.ghx round trip.");
    }
    public void Dispose() => doc.Dispose();
}
