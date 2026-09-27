using System.Drawing;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace Rhino2Midas.Grasshopper;

public sealed record DisplayOptions
{
    public bool Enabled { get; init; } = true;
    public bool Automatic { get; init; } = true;
    public bool Geometry { get; init; } = true;
    public bool Sections { get; init; } = true;
    public bool Loads { get; init; } = true;
    public bool Supports { get; init; } = true;
    public bool Springs { get; init; } = true;
    public bool Masses { get; init; } = true;
    public bool Releases { get; init; } = true;
    public bool Offsets { get; init; } = true;
    public bool OtherAttributes { get; init; } = true;
    public bool Axes { get; init; }
    public bool Labels { get; init; }
    public bool Values { get; init; } = true;
    public double SymbolScale { get; init; } = .2;
    public double LoadScale { get; init; } = .05;
    public string LoadPattern { get; init; } = "";
    public void Validate(){if(!double.IsFinite(SymbolScale)||SymbolScale<=0||!double.IsFinite(LoadScale)||LoadScale<=0)throw new ArgumentException("Display scales must be finite and positive.");}
}
public static class DisplayPreferences
{
    public static DisplayOptions Current { get; private set; } = new();
    public static event Action? Changed;
    public static void Set(DisplayOptions options,bool save=true)
    {
        options.Validate();Current=options;
        if(save)global::Grasshopper.Instances.Settings.SetValue("Rhino2Midas.Display",System.Text.Json.JsonSerializer.Serialize(options));
        Changed?.Invoke();Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();global::Grasshopper.Instances.ActiveCanvas?.Refresh();
    }
    public static void Load(){try{var json=global::Grasshopper.Instances.Settings.GetValue("Rhino2Midas.Display","");if(json.Length>0)Set(System.Text.Json.JsonSerializer.Deserialize<DisplayOptions>(json)!,false);}catch{Current=new();}}
}
public sealed class DisplayMenu:GH_AssemblyPriority
{
    public override GH_LoadingInstruction PriorityLoad(){DisplayPreferences.Load();Rhino.RhinoApp.Idle+=Install;return GH_LoadingInstruction.Proceed;}
    private static void Install(object? sender,EventArgs args)
    {
        var editor=global::Grasshopper.Instances.DocumentEditor;if(editor?.MainMenuStrip==null)return;
        if(editor.MainMenuStrip.Items.Cast<ToolStripItem>().Any(i=>i.Name=="Rhino2MidasMenu")){Rhino.RhinoApp.Idle-=Install;return;}
        var menu=new ToolStripMenuItem("Rhino2Midas"){Name="Rhino2MidasMenu"};
        menu.DropDownItems.Add("Visualizzazione…",null,(_,_)=>DisplayWindow.Open());
        menu.DropDownItems.Add("Guida",null,(_,_)=>DisplayWindow.Open(1));
        menu.DropDownItems.Add("About Rhino2Midas",null,(_,_)=>DisplayWindow.Open(2));
        menu.DropDownItems.Add(new ToolStripSeparator());
        var enabled=new ToolStripMenuItem("Preview attiva"){Checked=DisplayPreferences.Current.Enabled,CheckOnClick=true};
        enabled.CheckedChanged+=(_,_)=>DisplayPreferences.Set(DisplayPreferences.Current with{Enabled=enabled.Checked});
        menu.DropDownOpening+=(_,_)=>enabled.Checked=DisplayPreferences.Current.Enabled;menu.DropDownItems.Add(enabled);
        editor.MainMenuStrip.Items.Add(menu);Rhino.RhinoApp.Idle-=Install;
    }
}
public sealed class DisplayWindow:Form
{
    private static DisplayWindow? instance;
    private readonly TabControl tabs=new(){Dock=DockStyle.Fill};
    private readonly Dictionary<string,CheckBox> boxes=[];
    private readonly NumericUpDown symbols=new(){DecimalPlaces=3,Minimum=.001m,Maximum=100000m,Increment=.05m,Width=115};
    private readonly NumericUpDown loads=new(){DecimalPlaces=4,Minimum=.0001m,Maximum=100000m,Increment=.01m,Width=115};
    private readonly TextBox pattern=new(){Width=200};
    private bool syncing;
    public static void Open(int tab=0){if(instance==null||instance.IsDisposed)instance=new DisplayWindow();instance.tabs.SelectedIndex=tab;instance.Show(global::Grasshopper.Instances.DocumentEditor);instance.BringToFront();}
    public DisplayWindow()
    {
        Text="Rhino2Midas";Size=new Size(480,700);MinimumSize=new Size(420,540);StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;
        var heading=new Label{Text="Rhino2Midas",Font=new Font("Segoe UI",18,FontStyle.Bold),Height=58,Dock=DockStyle.Top,Padding=new Padding(16,10,0,0)};
        Controls.Add(tabs);Controls.Add(heading);
        var settings=new TabPage("Visualizzazione");var list=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(16)};settings.Controls.Add(list);tabs.TabPages.Add(settings);
        (string Name,string Label)[] switches=[("Enabled","Attiva tutte le preview Rhino2Midas"),("Automatic","Preview automatica degli output"),("Geometry","Geometria FEM: nodi, assi, mesh"),("Sections","Sezioni e spessori fisici"),("Loads","Carichi"),("Supports","Vincoli nodali e spostamenti imposti"),("Springs","Molle e link elastici"),("Masses","Masse nodali"),("Releases","Svincoli alle estremità"),("Offsets","Offset e punti di inserimento"),("OtherAttributes","Altri attributi e gruppi"),("Axes","Assi locali"),("Labels","Nomi elementi e proprietà"),("Values","Valori e descrizioni dei simboli")];
        foreach(var entry in switches){var box=new CheckBox{Text=entry.Label,AutoSize=true,Margin=new Padding(0,3,0,3)};boxes[entry.Name]=box;list.Controls.Add(box);box.CheckedChanged+=(_,_)=>Apply();}
        list.Controls.Add(new Label{Text="Scala simboli / assi [unità di lunghezza]",AutoSize=true,Margin=new Padding(0,12,0,2)});list.Controls.Add(symbols);
        list.Controls.Add(new Label{Text="Scala frecce carichi [lunghezza / valore]",AutoSize=true});list.Controls.Add(loads);
        list.Controls.Add(new Label{Text="Filtro load pattern (vuoto = tutti)",AutoSize=true});list.Controls.Add(pattern);
        var reset=new Button{Text="Ripristina predefiniti",AutoSize=true,Margin=new Padding(0,12,0,2)};reset.Click+=(_,_)=>DisplayPreferences.Set(new());list.Controls.Add(reset);
        symbols.ValueChanged+=(_,_)=>Apply();loads.ValueChanged+=(_,_)=>Apply();pattern.TextChanged+=(_,_)=>Apply();
        AddTextTab("Guida","Crea materiali, sezioni, elementi, attributi e carichi; unisci con Build Midas Model.\r\n\r\nExport e Analyze usano la REST API di Civil NX. Imposta MIDAS_API_URL e MIDAS_API_KEY dalla finestra API Settings di Civil. Salva il documento attivo prima di abilitare Replace active document. Run opera sul fronte false → true.\r\n\r\nPreview espone Breps solidi indipendentemente dalla visibilità. Le sezioni native complesse prive di geometria ricostruibile sono indicate in Issues.\r\n\r\nI risultati conservano colonne, assi e unità native; i nomi dei casi includono (ST), (CB), (CS). Le definizioni e gli input rimangono invariati.");
        AddTextTab("About","Rhino2Midas\r\nGrasshopper per Rhino 8 / .NET 8 / Windows x64\r\n\r\nModellazione, export e analisi MIDAS Civil NX tramite API REST. Componenti nativi generati dalla documentazione pubblica MIDAS, con riferimenti nel catalogo.\r\n\r\nLa connessione controlla il documento Civil NX attivo. Non avvia un solver isolato. Nessuna chiave API viene salvata nel file Grasshopper o negli archivi del modello.\r\n\r\nLe librerie proprietarie Rhino/MIDAS non sono distribuite.");
        DisplayPreferences.Changed+=Sync;FormClosed+=(_,_)=>DisplayPreferences.Changed-=Sync;Sync();
    }
    private void AddTextTab(string name,string text){var tab=new TabPage(name);tab.Controls.Add(new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,Text=text,Font=new Font("Segoe UI",10),BackColor=SystemColors.Window,BorderStyle=BorderStyle.None,Margin=new Padding(12)});tabs.TabPages.Add(tab);}
    private void Sync(){syncing=true;var current=DisplayPreferences.Current;foreach(var pair in boxes)pair.Value.Checked=(bool)typeof(DisplayOptions).GetProperty(pair.Key)!.GetValue(current)!;symbols.Value=(decimal)Math.Clamp(current.SymbolScale,.001,100000);loads.Value=(decimal)Math.Clamp(current.LoadScale,.0001,100000);pattern.Text=current.LoadPattern;syncing=false;}
    private void Apply(){if(syncing)return;var options=DisplayPreferences.Current with{SymbolScale=(double)symbols.Value,LoadScale=(double)loads.Value,LoadPattern=pattern.Text};foreach(var pair in boxes)typeof(DisplayOptions).GetProperty(pair.Key)!.SetValue(options,pair.Value.Checked);DisplayPreferences.Set(options);}
}
public sealed class DisplaySettingsComponent:SafeComponent
{
    public DisplaySettingsComponent():base("Midas Display Settings","Display Settings","Per-model preview switches. Connect to Preview Midas Model. Global preferences remain unchanged.","13-Preview"){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        foreach(string name in new[]{"Enabled","Geometry","Sections","Loads","Supports","Springs","Masses","Releases","Offsets","OtherAttributes","Axes","Labels","Values"})p.AddBooleanParameter(name,name,"Show "+name+".",GH_ParamAccess.item,name is not ("Axes" or "Labels"));
        p.AddNumberParameter("Symbol scale","S","Symbol and axis length.",GH_ParamAccess.item,.2);p.AddNumberParameter("Load scale","L","Arrow length per load value.",GH_ParamAccess.item,.05);p.AddTextParameter("Load pattern","P","Empty = all patterns.",GH_ParamAccess.item,"");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Settings","S","Local preview options.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var names=new[]{"Enabled","Geometry","Sections","Loads","Supports","Springs","Masses","Releases","Offsets","OtherAttributes","Axes","Labels","Values"};var options=new DisplayOptions{SymbolScale=Item<double>(da,13),LoadScale=Item<double>(da,14),LoadPattern=Item<string>(da,15)};for(int i=0;i<names.Length;i++)typeof(DisplayOptions).GetProperty(names[i])!.SetValue(options,Item<bool>(da,i));options.Validate();da.SetData(0,options);}
}
public sealed class DisplayPanelComponent:SafeComponent
{
    public DisplayPanelComponent():base("Rhino2Midas Panel","Rhino2Midas","Open the plugin window with About, Help and global display preferences. Also available in the Rhino2Midas menu.","13-Preview"){}
    protected override void RegisterInputParams(GH_InputParamManager p){}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddTextParameter("Help","H","How to open the panel.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>da.SetData(0,"Rhino2Midas menu → Visualizzazione, or right-click this component → Open Rhino2Midas.");
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu){base.AppendAdditionalComponentMenuItems(menu);Menu_AppendItem(menu,"Open Rhino2Midas",(_,_)=>DisplayWindow.Open());Menu_AppendItem(menu,"About",(_,_)=>DisplayWindow.Open(2));Menu_AppendItem(menu,"Help",(_,_)=>DisplayWindow.Open(1));}
}
