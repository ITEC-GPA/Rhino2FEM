using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using Rhino2SAP.Grasshopper;

string root=Path.GetFullPath(args.Length>0?args[0]:".");
var entries=JsonSerializer.Deserialize<Entry[]>(File.ReadAllText(Path.Combine(root,"docs","components.json")))!.OrderBy(e=>e.Category).ThenBy(e=>e.Name).ToArray();
string staging=args.Length>1?Path.GetFullPath(args[1]):Path.Combine(root,".local","icon-render-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(staging);
string assets=Path.Combine(staging,"Icons");Directory.CreateDirectory(assets);
Console.WriteLine($"Rendering {entries.Length} icons in {root}");int rendered=0;
foreach(var entry in entries){var icon=ComponentIcons.Get(entry.Name,entry.Category);if(icon.Width!=24||icon.Height!=24)throw new Exception("Invalid icon dimensions.");using var stream=new FileStream(Path.Combine(assets,entry.Class+".png"),FileMode.Create,FileAccess.Write,FileShare.Read);icon.Save(stream,ImageFormat.Png);if(++rendered%100==0)Console.WriteLine($"Icons: {rendered}");}
ComponentIcons.Get("Rhino2SAP","Model").Save(Path.Combine(assets,"Library.png"),ImageFormat.Png);
void Sheet(Entry[] rows,string filename,string subtitle)
{
    const int cols=4,cell=285,height=64,header=85;using var bitmap=new Bitmap(cols*cell,header+((rows.Length+cols-1)/cols)*height+15);
    using var g=Graphics.FromImage(bitmap);g.Clear(Color.FromArgb(246,248,251));using var title=new Font("Segoe UI",22,FontStyle.Bold);using var font=new Font("Segoe UI",9);using var small=new Font("Segoe UI",8);
    g.DrawString("Rhino2SAP",title,Brushes.Black,18,10);g.DrawString(subtitle,small,Brushes.DimGray,20,52);
    for(int i=0;i<rows.Length;i++){int x=i%cols*cell,y=header+i/cols*height;if(i/cols%2==0)g.FillRectangle(Brushes.White,x,y,cell,height);g.DrawImageUnscaled(ComponentIcons.Get(rows[i].Name,rows[i].Category),x+10,y+18);g.DrawString(rows[i].Name,font,Brushes.Black,new RectangleF(x+45,y+5,cell-54,38));g.DrawString(rows[i].Category,small,Brushes.DimGray,x+45,y+43);}
    bitmap.Save(Path.Combine(staging,filename),ImageFormat.Png);
}
var main=entries.Where(e=>!e.Class.Contains('_')).Concat(entries.Where(e=>e.Class.Contains('_')).GroupBy(e=>e.Category).SelectMany(g=>g.Take(2))).Take(64).ToArray();
Sheet(main,"icons.png",$"{entries.Length} componenti · icone native 24 × 24 px · selezione dei componenti principali");
for(int page=0;page*96<entries.Length;page++)Sheet(entries.Skip(page*96).Take(96).ToArray(),$"icons-{page+1:00}.png",$"Catalogo icone completo · pagina {page+1}");
Console.WriteLine($"Rendered {entries.Length+1} icons, overview and {(entries.Length+95)/96} full gallery pages in {staging}.");
record Entry(string Class,string Name,string Category,string Api,string File);
