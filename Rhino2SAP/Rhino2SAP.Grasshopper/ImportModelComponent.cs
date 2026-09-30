using Grasshopper.Kernel;
using Rhino2SAP.Api;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public sealed class ImportModelComponent : SafeComponent
{
    private bool wasRun;
    private SapModel? cached;
    private string? cachedPath;

    public ImportModelComponent() : base("Import SAP Model", "Import Model",
        "Read an existing SDB into an associated Grasshopper Model. Native definitions remain in the source; export/analysis preserve them using a verified private copy. Edit in SAP and reimport to update the Model.", "10-Import") { }

    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Path", "P", "Existing .sdb. Keep the source available for export and analysis.", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "R", "False to True imports a private copy. The source is never saved.", GH_ParamAccess.item, false);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "M", "Associated Model: preview, decompose, export or analyze to another SDB.", GH_ParamAccess.item);
        p.AddIntegerParameter("Units", "U", "Original SAP present units; geometry is not rescaled.", GH_ParamAccess.item);
        p.AddTextParameter("Cases", "C", "All native analysis case names.", GH_ParamAccess.list);
        p.AddTextParameter("Combinations", "Co", "All native response combination names.", GH_ParamAccess.list);
        p.AddTextParameter("Issues", "I", "Scope of the GH view and unavailable physical previews. Native data remains in the associated source.", GH_ParamAccess.list);
        p.AddTextParameter("Source hash", "H", "SHA256 of the associated source version.", GH_ParamAccess.item);
    }

    protected override void Solve(IGH_DataAccess da)
    {
        if (da.Iteration != 0) throw new ArgumentException("Use one SAP file per importer.");
        string path = Item<string>(da, 0);
        bool run = Item<bool>(da, 1), trigger = run && !wasRun;
        wasRun = run;
        if (path != cachedPath) cached = null;
        if (trigger)
        {
            cached = null; // A failed refresh must not publish an older snapshot.
            cached = WorkerClient.ImportModel(path);
            cachedPath = path;
        }
        if (cached == null) { Message = "Run to import"; return; }
        Output(da, 0, cached);
        da.SetData(1, cached.Units);
        da.SetDataList(2, cached.NativeSource!.Cases);
        da.SetDataList(3, cached.NativeSource.Combinations);
        da.SetDataList(4, cached.NativeSource.Issues);
        da.SetData(5, cached.NativeSource.FileHash);
        Message = "Associated SDB";
    }
}
