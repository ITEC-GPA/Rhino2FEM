using Grasshopper.Kernel;

namespace Rhino2SAP.Grasshopper;

public sealed class FilePathsComponent : SafeComponent
{
    public FilePathsComponent() : base("SAP File Paths", "SAP Paths",
        "Derive result archive and manifest paths from one SDB path. No files are read or written.", "26 Analisi e file") { }
    protected override void RegisterInputParams(GH_InputParamManager p) =>
        p.AddTextParameter("Path", "P", "SAP .sdb path.", GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("SDB", "S", "Absolute SAP model path.", GH_ParamAccess.item);
        p.AddTextParameter("Results archive", "R", "Archive created by analysis: .sdb.results.json.", GH_ParamAccess.item);
        p.AddTextParameter("Manifest", "M", "Provenance manifest: .sdb.rhino2sap.json.", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        string path = Path.GetFullPath(Item<string>(da, 0));
        if (!path.EndsWith(".sdb", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Choose a .sdb path.");
        da.SetData(0, path); da.SetData(1, path + ".results.json"); da.SetData(2, path + ".rhino2sap.json");
    }
}
