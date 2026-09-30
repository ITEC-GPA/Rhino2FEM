using System.Net;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

internal sealed record ExampleReport(string File,string Description,string Mode,int Objects,int Wires,int SapComponents,
    int ClosedBreps,string[] Topics,string[] ComponentTypes,string[] ModelFingerprints)
{
    public bool RoundTripGh => true;
    public bool RoundTripGhx => true;
    public bool AnalysisExecuted => false;
    public bool ActionsDisabled => true;
}

internal static class ExampleDocumentation
{
    public static void WriteGuide(string directory,Example example,GH_Component[] components)
    {
        var text=new StringBuilder("# "+example.Name.Replace('_',' ')+"\n\n");
        text.AppendLine(example.Description+"\n\nModalita: "+example.Mode+".\n");
        text.AppendLine($"[Apri GH]({example.Name}.gh) · [GHX]({example.Name}.ghx) · [Canvas]({example.Name.ToLowerInvariant()}.png)\n");
        text.AppendLine("## Passaggi sul canvas\n");
        foreach(var note in example.Graph.Document.Objects.OfType<GH_Panel>().Where(p=>p.SourceCount==0&&!string.IsNullOrWhiteSpace(p.UserText)))
            text.AppendLine("**"+note.NickName+"**\n\n"+note.UserText.Replace("\r","")+"\n");
        text.AppendLine("## Componenti e collegamenti\n\n| Componente | Gruppo Grasshopper | Input collegati |\n|---|---|---|");
        foreach(var c in components)
            text.AppendLine("| "+c.Name+" | "+c.SubCategory+" | "+string.Join("; ",c.Params.Input.Where(p=>p.SourceCount>0).Select(p=>p.Name+" ← "+string.Join(", ",p.Sources.Select(s=>s.Attributes.GetTopLevel.DocObject.NickName+"."+s.Name))))+" |");
        text.AppendLine("\n## Verifica effettuata\n\nSalvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.\n");
        File.WriteAllText(Path.Combine(directory,example.Name+".md"),text.ToString().TrimEnd()+"\n");
    }
    public static void WriteIndex(string directory,IReadOnlyList<ExampleReport> reports,IReadOnlyList<string> topics)
    {
        var text=new StringBuilder($"# Rhino2SAP — {reports.Count} esempi Grasshopper\n\n");
        text.AppendLine("Definizioni `.gh` e `.ghx` con geometria incorporata, gruppi, note, componenti collegati e preview PNG. [Galleria visuale](index.html).\n");
        text.AppendLine("Aprire il file GH con il plugin aggiornato in Rhino 8 / .NET 8. Leggere il canvas da sinistra a destra. Gli input sono incorporati nei componenti, con slider/pannelli dove utili; modificarli dal relativo menu Grasshopper. Unita comuni kN, m, C.\n");
        text.AppendLine("Gli esempi di modellazione funzionano senza avviare SAP. Export/import/analisi richiedono SAP2000 e una licenza API utilizzabile: scegliere un percorso e portare il relativo toggle da False a True. Overwrite resta False. Il generatore non ha eseguito analisi numeriche: risultati e query restano in attesa, senza valori simulati.\n");
        text.AppendLine("**Import Model** crea un Model associato al file SAP e ne controlla l'hash. La vista GH contiene geometria, proprieta supportate e assegnazioni comuni; export e analisi conservano tutte le definizioni native aprendo una copia privata del sorgente. Per modificare un modello associato, editarlo in SAP e reimportarlo. Conservare il file sorgente. **Read Geometry** importa invece solo la geometria. **Read Results Into Model** carica gli archivi `.sdb.results.json` del plugin, mentre **Read API Table** interroga direttamente il file SAP.\n");
        text.AppendLine("Le sezioni, i fattori, le funzioni dinamiche e le mesh sono didattici: gli esempi non eseguono verifiche normative. Le preview non convertono automaticamente tutte le proprieta native in solidi: consultare Issues.\n");
        text.AppendLine("## Raccolta\n\n| Esempio / guida | Argomento | Esecuzione |\n|---|---|---|");
        foreach(var r in reports){string stem=Path.GetFileNameWithoutExtension(r.File);text.AppendLine($"| [{stem.Replace('_',' ')}]({stem}.md) · [GH]({r.File}) | {r.Description} | {r.Mode} |");}
        text.AppendLine("\n## Copertura dei 34 gruppi\n\n| Gruppo | Esempi |\n|---|---|");
        foreach(string topic in topics)text.AppendLine("| "+topic+" | "+string.Join(", ",reports.Where(r=>r.Topics.Contains(topic)).Select(r=>"["+r.File[..2]+"]("+r.File+")"))+" |");
        text.AppendLine("\n## Rigenerazione e controlli\n\nDopo `./build.ps1`, eseguire `./tools/Generate-Examples.ps1`. Il catalogo, le guide, la galleria, le definizioni e `validation.json` derivano dalla stessa raccolta. Sono controllati i modelli anche dopo riapertura; il numero dei componenti e dei collegamenti deve coincidere. L'esecuzione e il bake devono rimanere disattivati. Il pacchetto `artifacts/Rhino2SAP_Examples.zip` contiene la raccolta. Gli eventuali esempi aggiunti da altri strumenti restano nella cartella e non vengono cancellati.\n");
        File.WriteAllText(Path.Combine(directory,"README.md"),text.ToString().TrimEnd()+"\n");
        string Escape(string value)=>WebUtility.HtmlEncode(value);
        var html=new StringBuilder("<!doctype html><html lang=it><meta charset=utf-8><meta name=viewport content='width=device-width,initial-scale=1'><title>Rhino2SAP — esempi</title><style>body{font:16px system-ui;background:#eef2f6;color:#172c42;margin:0}header{background:#102c47;color:white;padding:40px}main{padding:28px;max-width:1450px;margin:auto}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(330px,1fr));gap:22px}article{background:white;padding:20px;border-radius:12px;box-shadow:0 3px 14px #15365212}img{width:100%;height:210px;object-fit:contain;background:#f6f8fa}a{color:#086791}h2{font-size:19px}small{color:#586879}.tags{font-size:12px;color:#506778}input{padding:12px;width:calc(100% - 26px);margin:0 0 25px;border:1px solid #bac8d5;border-radius:8px}</style><header><h1>Rhino2SAP</h1><p>"+reports.Count+" esempi Grasshopper · 34 gruppi · GH + GHX + guide</p><p>Geometrie incorporate. Run e Bake spenti. Analisi numeriche SAP non eseguite.</p></header><main><input id=q aria-label='Cerca esempio' placeholder='Cerca: mesh, link, import, analisi, risultati…'><div class=grid>");
        foreach(var r in reports)
        {
            string stem=Path.GetFileNameWithoutExtension(r.File);
            html.Append($"<article><a href='{stem.ToLowerInvariant()}.png'><img loading=lazy src='{stem.ToLowerInvariant()}.png' alt='Canvas {Escape(stem)}'></a><h2>{Escape(stem.Replace('_',' '))}</h2><p>{Escape(r.Description)}</p><small>{Escape(r.Mode)}</small><p><a href='{r.File}'>Apri GH</a> · <a href='{stem}.ghx'>GHX</a> · <a href='{stem}.md'>Guida</a></p><p class=tags>{Escape(string.Join(" · ",r.Topics))}</p></article>");
        }
        html.Append("</div></main><script>document.getElementById('q').addEventListener('input',e=>{const q=e.target.value.toLocaleLowerCase();document.querySelectorAll('article').forEach(a=>a.hidden=!a.textContent.toLocaleLowerCase().includes(q))})</script></html>");
        File.WriteAllText(Path.Combine(directory,"index.html"),html.ToString());
    }
}
