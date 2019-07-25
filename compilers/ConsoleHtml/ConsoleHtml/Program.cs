using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using iTextSharp;
//using iTextSharp.text.pdf;
using Newtonsoft.Json;

namespace ConsoleHtml
{
    class Program
    {
        static void print(object o)
        {
            Console.WriteLine(o);
        }
        static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");
            print("usage: dotnet ConsoleHtml <path to .json files> or <.json file>");

            if (args.Length>0)
            {
                if (File.Exists(args[0]))
                {
                    print($"processing {args[0]}");
                    process(args[0]);
                }
                else if (Directory.Exists(args[0]))
                {
                    foreach(var file in Directory.GetFiles(args[0],"*.json"))
                    {
                        try
                        {
                            print($"processing {file}");
                            process(file);
                        }
                        catch(Exception ex)
                        {
                            print($"{args[0]} processing error {ex.Message}");
                        }
                    }
                }
            }
            
            return;
            var rnd = new Random();
            var cheatsheet = new Dictionary<string, object>();
            cheatsheet.Add("title", "Test Cheat Sheet Name");
            cheatsheet.Add("forecolor", "black");
            cheatsheet.Add("backcolor", "white");
            cheatsheet.Add("css", "");
            cheatsheet.Add("categories", new List<object>());
            for(var c=0;c<rnd.Next(5,10);c++)
            {
                var category = new Dictionary<string, object>();
                category.Add("title", $"Category {c+1}");
                category.Add("css", "padding:1%;background:orange;");
                category.Add("items", new List<object>());
                for (int i = 0; i < rnd.Next(3,10); i++)
                {
                    var item = new Dictionary<string, string>();

                    item.Add("text", $"random text {rnd.Next(0, 99999)} {((rnd.Next(0,4)>2)?"[img]urltoimage[/img]":"")}");
                    item.Add("order", $"{i}");
                    item.Add("css", "");
                    (category["items"] as List<object>).Add(item);
                }

                 (cheatsheet["categories"] as List<object>).Add(category);
            }

            File.WriteAllText("cheatsheetdefinition.json",JsonConvert.SerializeObject(cheatsheet));
       
            
        }

 

        static void process(string csdfPath)
        {
            var cheatsheet = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(csdfPath));
            if (!cheatsheet.ContainsKey("css"))
            {
                cheatsheet.Add("css", "");

            }
            if (!cheatsheet.ContainsKey("title"))
            {
                cheatsheet.Add("title", $"Cheat Sheet {DateTime.Now.ToShortDateString()}");

            }

            if (!cheatsheet.ContainsKey("forecolor"))
            {
                cheatsheet.Add("forecolor", "black");

            }
            if (!cheatsheet.ContainsKey("backcolor"))
            {
                cheatsheet.Add("backcolor", "white");

            }
            var html = new StringBuilder();

            html.AppendLine("<html><head><meta charset='utf-8'>");
            html.AppendLine($"<style>");
            html.AppendLine(@"
.container {
 display: grid ;
grid-column-gap:1%;
grid-template-columns: 49% 49%;

}
.item{
 text-align: justify;
  text-justify: inter-word;
}
");// grid-template-columns: repeat(auto-fill, minmax(20%, 1fr));


            html.AppendLine("body {color:" + cheatsheet["forecolor"]+";");
            html.AppendLine("background-color:" + cheatsheet["backcolor"]+";}");
            html.AppendLine($"{cheatsheet["css"]}");
            html.AppendLine("</style>");
            html.AppendLine("</head>");

            html.AppendLine("<body>");
            html.AppendLine($"<h1>{cheatsheet["title"]}</h1>");
          
            var categories = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(""+cheatsheet["categories"]);
           
            html.AppendLine("<div class='container'>");
            int catNum = 0;
            foreach (var cat in categories)
            {
                if (!cat.ContainsKey("css"))
                {
                    cat.Add("css", "background:orange;padding:5px;");
                }
                if (cat["css"].ToString().Contains("optional"))
                {
                    cat["css"] = "background:orange;padding:5px;";
                }

                html.AppendLine($@"<div class='item'>"); //grid item

                html.AppendLine($"<h2 style='{cat["css"]}'>{cat["title"]}</h2>");
                var items = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(""+cat["items"]);
                
                foreach (var item in items.OrderBy(itm => itm["order"]))
                {
                    if (!item.ContainsKey("css"))
                    {
                        item.Add("css", "padding:5px;");
                    }
                    if (item["css"].ToString().Contains("optional"))
                    {
                        item["css"] = "padding:5px;";
                    }
                    var text = item["text"];
                    if (text.ToLower().Contains("[img]") && text.ToLower().Contains("[/img]") )
                    {
                        text = ReplaceImgWithImg(text);
                         

                    }
                    html.AppendLine($"<div style='{item["css"]}'>{text}</div>");
                }
                html.AppendLine("</div>"); //grid item
                catNum++;
            }
            html.AppendLine("</div>"); //.container
            html.AppendLine("</body></html>");

            File.WriteAllText($"{csdfPath}.html", html.ToString());

        }

        public static (string text,string tagValue) GetBetweenTags(string text, string tagStart, string tagEnd)
        {
            string result=text;
            int left = text.ToLower().IndexOf(tagStart.ToLower()) + tagStart.Length;
            int right = text.ToLower().IndexOf(tagEnd.ToLower());
            
            return (text: text.Replace(result,""),tagValue:result) ;
        }

        public static string ReplaceImgWithImg(string text)
        {
            var tagStart = "[img]";
            var tagEnd = "[/img]";
           
            text = text.Replace("[IMG]", "[img]").Replace("[/IMG]", "[/img]");
          //  print("replacing images");

            while (text.ToLower().IndexOf(tagStart.ToLower())>-1 && text.ToLower().IndexOf(tagEnd.ToLower()) > -1)
            {
                int left = text.ToLower().IndexOf(tagStart.ToLower());
                int right = text.ToLower().IndexOf(tagEnd.ToLower())+tagEnd.Length;
                var linkBlock = text.Substring(left, right - left);
                var link = linkBlock.Replace("[img]", "").Replace("[/img]","");
                text = text.Replace(linkBlock, $"<img style='float:left;padding:3px;'  width='75px' src='{link}'/>");
          //      print($"{text} and {link}");
            }

            return $"<div style='display: inline-block;'>{text}</div>";



        }
    }
}
