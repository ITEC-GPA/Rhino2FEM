using System.Drawing;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;

// A small, named-socket DSL: examples describe the engineering workflow, not canvas coordinates.
internal sealed class TopicGraph
{
    public GraphBuilder G { get; }
    public string Name { get; }
    public string Description { get; }
    private int column = -1, y;
    private string stage = "";
    private List<IGH_DocumentObject> stageObjects = [];
    private static readonly Color[] Colours = [Color.SteelBlue, Color.MediumPurple, Color.DarkOrange, Color.MediumSeaGreen, Color.CadetBlue];
    private int X => 50 + column * 620;

    public TopicGraph(Assembly assembly, string name, string description)
    {
        Name = name; Description = description;
        G = new(assembly, name.Replace('_', ' '));
        G.Note("RHINO2SAP / " + name.Replace('_', ' '),
            description + "\nUnita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.\n" +
            "I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.",
            50, 20, 1800, 145);
    }

    public void Step(string title, string instructions)
    {
        CloseStage(); column++; stage = $"{column + 1:00}  {title}";
        stageObjects = [G.Note(stage, instructions, X, 220, 545, 155)];
        y = 430;
    }
    private void CloseStage()
    {
        if (stageObjects.Count != 0) G.Group(stage, Colours[column % Colours.Length], stageObjects.ToArray());
    }
    public GH_Component Add(string type, string label, params (string Name, object Value)[] values)
    {
        var component = G.Component(type, X + 245, y, label);
        Set(component, values); component.Attributes.PerformLayout();
        var pivot=component.Attributes.Pivot;
        component.Attributes.Pivot=new PointF(pivot.X,pivot.Y+y-component.Attributes.Bounds.Top);
        component.Attributes.ExpireLayout();component.Attributes.PerformLayout();
        stageObjects.Add(component);
        y = (int)Math.Ceiling(component.Attributes.Bounds.Bottom)+80;
        return component;
    }
    public GH_Component Api(string key, params (string Name, object Value)[] values) =>
        Add(key.Replace('.', '_') + "Component", key[(key.LastIndexOf('.') + 1)..], values);
    public void Set(GH_Component component, params (string Name, object Value)[] values)
    {
        foreach (var (name, value) in values)
            G.Values(component, name, value is Array array ? array.Cast<object>().ToArray() : [value]);
    }
    public void Wire(GH_Component source, string output, GH_Component target, string input) =>
        GraphBuilder.Input(target, input).AddSource(source.Params.Output.Single(p => p.Name == output));
    public void Link(GH_Component source, GH_Component target, string input = "Definitions") => G.Wire(source, 0, target, input);
    public GH_Panel Panel(string title, string value, bool list = false)
    {
        var panel = G.Note(title, value, X, y, 545, 130);
        panel.Properties.Multiline = !list; stageObjects.Add(panel); y += 165; return panel;
    }
    public void Output(GH_Component source, string output, string label)
    {
        var panel = Panel(label, ""); panel.AddSource(source.Params.Output.Single(p => p.Name == output));
    }
    public GH_BooleanToggle Toggle(string label, bool value = false)
    {
        var toggle = G.Toggle(label, value, X + 60, y); stageObjects.Add(toggle); y += 85; return toggle;
    }
    public void Slider(GH_Component target, string input, decimal value, decimal min, decimal max, string label)
    {
        var slider = G.Slider(label, value, min, max, X + 40, y);
        stageObjects.Add(slider); G.Wire(slider, target, input); y += 75;
    }
    public void BrepCast(GH_Component source)
    {
        var brep=G.Place(new Param_Brep{NickName="Cast beam -> Brep"},X+245,y+30);
        brep.AddSource(source.Params.Output[0]);stageObjects.Add(brep);y+=110;
    }
    public GH_Component Model(params GH_Component[] definitions)
    {
        Step("MODELLO", "Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.");
        var model = Add("BuildModelComponent", "Build Model");
        foreach (var definition in definitions) Link(definition, model);
        return model;
    }
    public void Preview(GH_Component model)
    {
        Step("CONTROLLO / PREVIEW", "Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.");
        var valid = Add("ValidateModelComponent", "Valida"); Link(model, valid, "Model"); Output(valid, "Valid", "Valid");
        var display = Add("DisplaySettingsComponent", "Visualizzazione", ("Labels", true), ("Axes", true));
        var toggle = Toggle("PREVIEW", true); G.Wire(toggle, display, "Enabled");
        var preview = Add("PreviewModelComponent", "Preview / Brep"); Link(model, preview, "Model"); Link(display, preview, "Settings");
        Output(preview, "Status", "Stato / Brep"); Output(preview, "Issues", "Limiti della ricostruzione geometrica");
    }
    public GH_Component Execute(GH_Component model, string[] cases, bool export = false, string[]? quantities = null)
    {
        Step(export ? "EXPORT SDB" : "ANALISI SAP", "Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.");
        var path = Panel("File di destinazione", @"C:\Users\Public\Documents\Rhino2SAP_" + Name + ".sdb");
        var trigger = Toggle(export ? "EXPORT SAP" : "RUN SAP");
        var run = Add(export ? "ExportModelComponent" : "AnalyzeAndEmbedComponent", export ? "Export" : "Analisi e risultati");
        Link(model, run, "Model"); G.Wire(path, run, "Path"); G.Wire(trigger, run, "Run");
        if (cases.Length != 0) Set(run, ("Cases", cases));
        if (quantities != null) Set(run, ("Quantities", quantities));
        Output(run, "Issues", "Esito lettura risultati"); return run;
    }
    public Example Finish(int breps, bool requiresModel = true, string mode = "Modellazione locale")
    {
        CloseStage(); return new(Name, G, breps, Description, requiresModel, mode);
    }
}

internal static partial class TopicExamples
{
    private static readonly bool[] Fixed = [true, true, true, true, true, true];
    private static readonly double[] Zero = [0, 0, 0, 0, 0, 0];
    private sealed record BeamParts(GH_Component Frame, GH_Component Support, GH_Component Tip, GH_Component Pattern, GH_Component Load);

    private static GH_Component Material(TopicGraph g, bool concrete = false) => g.Add("MaterialComponent", concrete ? "Calcestruzzo" : "Acciaio",
        ("Name", concrete ? "C30_DEMO" : "STEEL"), ("Type", concrete ? 2 : 1), ("E", concrete ? 30e6 : 210e6), ("Poisson", concrete ? .2 : .3), ("Weight density", concrete ? 25d : 78.5));
    private static GH_Component Rectangle(TopicGraph g, GH_Component material, string name = "R20x10")
    {
        var section = g.Api("PropFrame.SetRectangle", ("Name", name), ("T3", .2), ("T2", .1));
        g.Link(material, section, "MatProp"); return section;
    }
    private static GH_Component Shell(TopicGraph g, GH_Component material)
    {
        var shell = g.Api("PropArea.SetShell_1", ("Name", "SHELL20"), ("ShellType", 1), ("IncludeDrillingDOF", true), ("MatAng", 0d), ("Thickness", .2), ("Bending", .2));
        g.Link(material, shell, "MatProp"); return shell;
    }
    private static GH_Component Node(TopicGraph g, string name, Point3d point) => g.Add("NodeElementComponent", name, ("Name", name), ("Point", point));
    private static GH_Component Support(TopicGraph g, GH_Component node, bool[]? fixedDof = null)
    {
        var support = g.Api("PointObj.SetRestraint", ("Value", fixedDof ?? Fixed)); g.Link(node, support, "Name"); return support;
    }
    private static GH_Component Pattern(TopicGraph g, string name = "Q", int type = 8, double weight = 0) =>
        g.Add("LoadPatternComponent", name, ("Name", name), ("Type", type), ("Self weight", weight));
    private static BeamParts Beam(TopicGraph g, bool vertical = false)
    {
        g.Step("MATERIALE / SEZIONE", "Sezione rettangolare 0.20 x 0.10 m. E=210e6 kN/m2. I nomi delle definizioni devono essere univoci.");
        var section = Rectangle(g, Material(g));
        g.Step("ELEMENTI / VINCOLO", "Mensola di 3 m. Incastro al nodo BASE. Il nodo TIP coincide con l'estremita del frame F1.");
        var end = vertical ? new Point3d(0, 0, 3) : new Point3d(3, 0, 0);
        var frame = g.Add("FrameElementComponent", "Frame F1", ("Name", "F1"), ("Line", new Line(Point3d.Origin, end)));
        g.Link(section, frame, "Property");
        var support = Support(g, Node(g, "BASE", Point3d.Origin));
        var tip = Node(g, "TIP", end);
        g.Step("PATTERN / CARICO", "Pattern Q senza peso proprio. Forza al TIP: -1 kN lungo Z per la mensola orizzontale, +1 kN lungo X per quella verticale.");
        var pattern = Pattern(g);
        var load = g.Api("PointObj.SetLoadForce", ("Value", vertical ? new double[] {1,0,0,0,0,0} : new double[] {0,0,-1,0,0,0}), ("CSys", "Global"));
        g.Link(tip, load, "Name"); g.Link(pattern, load, "LoadPat");
        return new(frame, support, tip, pattern, load);
    }
    private static GH_Component BeamModel(TopicGraph g, BeamParts b, params GH_Component[] extras) => g.Model([b.Frame, b.Support, b.Load, .. extras]);
    private static GH_Component MeshModel(TopicGraph g, bool triangles = false)
    {
        g.Step("MATERIALE / SHELL", "Shell sottile, 20 cm; materiale elastico didattico. Spessore membranale e flessionale uguali.");
        var shell = Shell(g, Material(g, true));
        g.Step("MESH / APPOGGI", "Quadrato 4 x 4 m discretizzato 2 x 2. La mesh e incorporata; ogni faccia diventa un oggetto SAP. Gli otto nodi di bordo sono incastrati.");
        var mesh = new Mesh();
        for (int y=0;y<3;y++) for (int x=0;x<3;x++) mesh.Vertices.Add(2*x,2*y,0);
        for (int y=0;y<2;y++) for (int x=0;x<2;x++) { int i=3*y+x; if(triangles){mesh.Faces.AddFace(i,i+1,i+4);mesh.Faces.AddFace(i,i+4,i+3);}else mesh.Faces.AddFace(i,i+1,i+4,i+3); }
        mesh.Normals.ComputeNormals();
        var areas = g.Add("MeshAreasComponent", triangles ? "8 triangoli" : "4 quadrilateri", ("Mesh", mesh)); g.Link(shell, areas, "Property");
        var boundary=mesh.Vertices.Select(p=>new Point3d(p.X,p.Y,p.Z)).Where(p=>p.X==0||p.X==4||p.Y==0||p.Y==4).ToArray();
        var nodes=g.Add("NodeElementComponent","Bordo",("Name",Enumerable.Range(1,boundary.Length).Select(i=>"B"+i).ToArray()),("Point",boundary));
        var fix=Support(g,nodes);
        g.Step("PRESSIONE", "-5 kN/m2 in Z globale (Dir=6). Name=ALL, ItemType=1 assegna il carico al gruppo ALL; Definitions porta la mesh nello stesso ramo.");
        var pattern=Pattern(g);
        var load=g.Api("AreaObj.SetLoadUniform",("Name","ALL"),("ItemType",1),("Dir",6),("Value",-5d),("CSys","Global"));
        g.Link(areas,load);g.Link(pattern,load,"LoadPat"); return g.Model(load,fix);
    }
    private static GH_Component Reader(TopicGraph g, GH_Component solved, string method, string? column = null)
    {
        var reader=g.Add(method.Replace('.','_')+"Component",method[8..]);g.Link(solved,reader,"Model");
        if(column!=null)g.Output(reader,column,column+" / unita del modello");return reader;
    }
}
