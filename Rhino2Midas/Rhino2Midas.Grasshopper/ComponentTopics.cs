namespace Rhino2Midas.Grasshopper;

/// <summary>Shared topic map for the ribbon, generated API components and documentation.</summary>
public static class ComponentTopics
{
    public static string For(string name,string category)
    {
        string n=name.ToLowerInvariant();
        if(category=="13-Preview")return "29 Preview e bake";
        if(category=="07-Model")return "01 Modello";
        if(category is "09-Export" or "10-Import" or "11-Analysis"||n.Contains("api connection"))return "22 Analisi e file";
        if(category=="15-Native Results"||category=="12-Results")
        {
            if(n.Contains("plate"))return "25 Risultati piastre";
            if(n.Contains("beam")||n.Contains("truss"))return "24 Risultati travi e aste";
            if(n.Contains("solid")||n.Contains("link"))return "26 Risultati solidi e link";
            if(n.Contains("eigen")||n.Contains("buckling")||n.Contains("mode")&&!n.Contains("model"))return "27 Risultati modali";
            if(n.Contains("node")||n.Contains("reaction")||n.Contains("displacement")||n.Contains("acceleration")||n.Contains("velocity"))return "23 Risultati nodali";
            return "28 Risultati e query";
        }
        if(n.Contains("align")||n.Contains("offset")||n.Contains("local axis")||n.Contains("axes"))return "09 Assi e offset";
        if(category=="08-Tools")return "30 Utility";
        if(category=="03-Elements")return "06 Elementi";
        if(category=="01-Materials"||n.Contains("material")||n.Contains("creep")||n.Contains("shrinkage"))return "02 Materiali";
        if(n.Contains("combination"))return "12 Combinazioni";
        if(n.Contains("mass"))return "10 Masse";
        if(n.Contains("tendon")||n.Contains("prestress")||n.Contains("pretension"))return "17 Precompressione";
        if(n.Contains("temperature")||n.Contains("hydration")||n.Contains("heat ")||n.Contains("convection")||n.Contains("cooling"))return "18 Temperatura e idratazione";
        if(n.Contains("construction")||n.Contains("stage"))return "19 Fasi costruttive";
        if(n.Contains("vehicle")||n.Contains("moving")||n.Contains("traffic")||n.Contains("lane")||n.Contains("train")||n.Contains("influence"))return "16 Carichi mobili";
        if(n.Contains("time history")||n.Contains("ground acceleration")||n.Contains("time varying")||n.Contains("dynamic"))return "21 Time history";
        if(n.Contains("spectrum")||n.Contains("eigen")||n.Contains("seismic")||n.Contains("response")||n.Contains("modal"))return "20 Modale e sisma";
        if(n.Contains("load case")||n.Contains("static case"))return "11 Casi di carico";
        if(n.Contains("nodal load")||n.Contains("node load")||n.Contains("body force")||n.Contains("self weight"))return "13 Carichi nodali e peso";
        if(n.Contains("beam")&&(n.Contains("load")||n.Contains("moment")))return "14 Carichi travi";
        if(n.Contains("pressure")||n.Contains("plate load")||n.Contains("floor load")||n.Contains("plane load"))return "15 Carichi piastre";
        if(n.Contains("section")&&!n.Contains("section scale"))return "03 Sezioni travi";
        if(n.Contains("thickness")||n.Contains("plate stiffness"))return "04 Piastre e solidi";
        if(n.Contains("elastic link")||n.Contains("general link")||n.Contains("cable"))return "05 Link e cavi";
        if(n.Contains("support")||n.Contains("spring")||n.Contains("release")||n.Contains("rigid link")||n.Contains("constraint")||n.Contains("specified displacement"))return "07 Vincoli e molle";
        if(n.Contains("analysis")||n.Contains("control")||n.Contains("buckling")||n.Contains("p-delta")||n.Contains("nonlinear"))return "22 Analisi e file";
        if(category=="02-Properties")return "04 Piastre e solidi";
        if(category=="04-Attributes"||n.Contains("group"))return "08 Gruppi e attributi";
        if(category=="05-Loads"||n.Contains("load"))return "15 Carichi piastre";
        if(category=="06-Cases")return "11 Casi di carico";
        return "30 Utility";
    }
}
