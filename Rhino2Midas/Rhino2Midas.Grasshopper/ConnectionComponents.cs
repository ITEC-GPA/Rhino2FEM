using System.Windows.Forms;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino2Midas.Api;
using Rhino2Midas.Core;

namespace Rhino2Midas.Grasshopper;

public static class ConnectionSettings
{
    private static string? apiKey;
    private static string? url;
    public static CivilClient Create()=>new(url??Environment.GetEnvironmentVariable("MIDAS_API_URL")??"",apiKey??Environment.GetEnvironmentVariable("MIDAS_API_KEY")??"");
    public static void Open()
    {
        var form=new Form{Text="Rhino2Midas — Civil NX API",Size=new Size(540,330),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false};
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(16),ColumnCount=1,RowCount=7};
        layout.Controls.Add(new Label{AutoSize=true,Text="Civil NX → Apps → API Settings → Connect"});
        layout.Controls.Add(new Label{AutoSize=true,Text="Base URL"});var address=new TextBox{Dock=DockStyle.Top,Text=url??Environment.GetEnvironmentVariable("MIDAS_API_URL")??""};layout.Controls.Add(address);
        layout.Controls.Add(new Label{AutoSize=true,Text="MAPI-Key (solo per questa sessione Rhino)"});var key=new TextBox{Dock=DockStyle.Top,UseSystemPasswordChar=true,Text=apiKey??Environment.GetEnvironmentVariable("MIDAS_API_KEY")??""};layout.Controls.Add(key);
        var message=new Label{AutoSize=true,MaximumSize=new Size(470,70),Text="La connessione controlla il documento Civil attivo. La chiave non viene salvata in Grasshopper."};layout.Controls.Add(message);
        var button=new Button{Text="Verifica e usa connessione",AutoSize=true};layout.Controls.Add(button);
        button.Click+=async(_,_)=>{button.Enabled=false;try{using var client=new CivilClient(address.Text,key.Text,timeout:TimeSpan.FromSeconds(15));await client.ReadDatabaseAsync("UNIT");url=address.Text;apiKey=key.Text;message.Text="Connesso. Impostazioni valide fino alla chiusura di Rhino.";}catch(Exception ex){message.Text=ex.Message;}finally{button.Enabled=true;}};
        form.Controls.Add(layout);form.ShowDialog(global::Grasshopper.Instances.DocumentEditor);
    }
}
public sealed class ApiConnectionComponent:SafeComponent
{
    public ApiConnectionComponent():base("Midas API Connection","Connection","Open Civil NX API connection settings. Credentials remain in memory, or come from MIDAS_API_URL / MIDAS_API_KEY environment variables.","09-Export"){}
    protected override void RegisterInputParams(GH_InputParamManager p){}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddTextParameter("Help","H","Connection instructions.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>da.SetData(0,"Right-click → Configure Civil NX API. In Civil: Apps → API Settings → Connect. Save Civil work before using Replace active document.");
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu){base.AppendAdditionalComponentMenuItems(menu);Menu_AppendItem(menu,"Configure Civil NX API",(_,_)=>ConnectionSettings.Open());}
}
public sealed class ReadApiDatabaseComponent:SafeComponent
{
    private bool wasRun;private string? cached;private string? lastEndpoint;
    public ReadApiDatabaseComponent():base("Read Midas API Database","Read API","Read one native database table from the active Civil model.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Endpoint","E","Table name, e.g. MATL.",GH_ParamAccess.item,"MATL");p.AddBooleanParameter("Run","R","Read on rising edge.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("JSON","J","Native table keyed by IDs.",GH_ParamAccess.item);p.AddGenericParameter("Definitions","D","Native records as fragments.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){string endpoint=Item<string>(da,0).ToUpperInvariant();bool run=Item<bool>(da,1);if(lastEndpoint!=endpoint)cached=null;bool trigger=run&&!wasRun;wasRun=run;if(trigger){cached=null;using var client=ConnectionSettings.Create();cached=client.ReadDatabaseAsync(endpoint).GetAwaiter().GetResult().GetRawText();lastEndpoint=endpoint;}if(cached!=null){da.SetData(0,cached);using var doc=System.Text.Json.JsonDocument.Parse(cached);da.SetDataList(1,doc.RootElement.EnumerateObject().Select(p=>new FragmentGoo(new(entries:[new(endpoint,int.Parse(p.Name),p.Value)]))));}}
}
public sealed class ReadNativeResultTableComponent:SafeComponent
{
    private bool wasRun;private IReadOnlyList<ResultTable>? cached;private string? key;
    public ReadNativeResultTableComponent():base("Read Midas Native Result Table","Read Table","Read requested results from the active Civil model. This direct table is not automatically attributed to a Grasshopper Model.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Request","Q","Native Result Request.",GH_ParamAccess.item);p.AddGenericParameter("Units","U","Optional Model Units for result extraction; default KN/M/KJ/C. Use the same units as your model.",GH_ParamAccess.item);p[1].Optional=true;p.AddBooleanParameter("Run","R","Read on rising edge.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Tables","T","Unattributed native tables with requested units.",GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da){var q=Native<TableRequest>(da,0);object rawUnits=null!;var u=da.GetData(1,ref rawUnits)?Unwrap(rawUnits) as Units??throw new ArgumentException("Use Model Units."):new Units();u.Validate();bool run=Item<bool>(da,2);string current=ModelArchive.Json(q)+u;if(key!=current)cached=null;bool trigger=run&&!wasRun;wasRun=run;if(trigger){cached=null;using var client=ConnectionSettings.Create();cached=ResultTable.ParseResponse(client.SendAsync("POST","post/TABLE",q.Body(u)).GetAwaiter().GetResult(),q.Type);key=current;}if(cached!=null)da.SetDataList(0,cached);}
}
