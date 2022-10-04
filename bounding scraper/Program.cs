using Newtonsoft.Json;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;

List<string> urls = new List<string> () { "https://bounding.net/a.txt", "https://bounding.net/b.txt" };
List<string> maps = new List<string> ();
int maps_found = 0;
int map_processed = 0;
int active_threads = 0;
int max_threads = 20;
List<Map> outputMap = new List<Map>();
void GetMapsList ()
{
    int i = 0;
    foreach (string url in urls)
    {
        using (WebClient client = new WebClient())
        {
            
            client.DownloadFile(url, i + ".txt");
            maps.AddRange(File.ReadAllLines (i + ".txt").ToList());
            File.Delete(i + ".txt");
        }
        i++;
    }
}

void ConsoleAdjust (Point xy, ConsoleColor color)
{
    Console.SetCursorPosition(xy.X, xy.Y);
    Console.ForegroundColor = color;
}

void SetConsoleValue(string value, string location)
{
    switch (location)
    {
        case "urls_loaded":
            ConsoleAdjust(new Point(12, 0), ConsoleColor.Cyan);
            break;
        case "maps_found":
            ConsoleAdjust(new Point(12, 1), ConsoleColor.Cyan);
            break;
        case "maps_processed":
            ConsoleAdjust(new Point(16, 3), ConsoleColor.Cyan);
            break;
        case "maps_downloaded":
            ConsoleAdjust(new Point(17, 4), ConsoleColor.Cyan);
            break;
        case "active_threads":
            ConsoleAdjust(new Point(16, 2), ConsoleColor.Cyan);
            break;
    }
    Console.Write(value);
    Console.CursorTop = Console.CursorTop + 1;
    Console.CursorLeft = 0;
}
 void SetVisuals()
{
    // x = 12
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Urls Loaded: ");
    SetConsoleValue("0", "urls_loaded");
    // x = 12
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Maps Found: ");
    SetConsoleValue("0", "maps_found");
    // x = 16
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Active Threads: ");
    SetConsoleValue("0", "active_threads");
    // x = 16
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Maps Processed: ");
    SetConsoleValue("0", "maps_processed");
    // x = 17
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Maps Downloaded: ");
    SetConsoleValue("0", "maps_downloaded");
}

List<Map> getDownloadUrls(string url)
{

again:
    try
    {
        
        List<Map> output = new List<Map>();
        using (HttpClient client = new HttpClient())
        {
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            {
                using (HttpContent content = response.Content)
                {
                    string result = content.ReadAsStringAsync().Result;
                    string a = Regex.Split(result, "<h3>Known Filenames</h3>").ToList()[1];
                    string b = Regex.Split(a, "</table>").ToList()[0];
                    List<string> c = Regex.Split(b, "<a download=\"").ToList();
                    c.RemoveAt(0);
                    string filename = Regex.Split(c[0], "\"").ToList()[0];
                    string d = Regex.Split(c[0], "href=\"").ToList()[1];
                    string link = Regex.Split(d, "\">Download</a>").ToList()[0];
                    Map map = new Map();
                    map.Filename = filename;
                    map.Download = "https://bounding.net" + link;
                    output.Add(map);
                    maps_found++;
                }
            }
            return output;
        }
    } catch (Exception ex)
    {
        goto again;
    }
    


    
}
SetVisuals ();
GetMapsList ();

SetConsoleValue(maps.Count.ToString(), "urls_loaded");
thread();
//Thread getUrls = new Thread(thread);
//getUrls.Start();


void thread () {
    int i = 0;
    do
    {
        string map = maps[i];
        i++;
        do
        {
            SetConsoleValue(map_processed.ToString(), "maps_processed");
            SetConsoleValue(active_threads.ToString(), "active_threads");
            SetConsoleValue(maps_found.ToString(), "maps_found");
            wait(0.01);
        } while (active_threads >= max_threads);
        Thread geturlsthreaded = new Thread(() => processWebsite(map));
        geturlsthreaded.Start();
        active_threads++;
        SetConsoleValue(map_processed.ToString(), "maps_processed");
        SetConsoleValue(active_threads.ToString(), "active_threads");
        SetConsoleValue(maps_found.ToString(), "maps_found");
    } while (i < maps.Count);
    
}

void processWebsite(string map)
{
    outputMap.AddRange(getDownloadUrls(map));

    map_processed++;

    active_threads--;

}

File.WriteAllText(@"map output.json", JsonConvert.SerializeObject(outputMap));

void wait(double x)
{
    DateTime t = DateTime.Now;
    DateTime tf = DateTime.Now.AddSeconds(x);

    while (t < tf)
    {
        t = DateTime.Now;
    }
}

public class Map
{
    public string Filename { get; set; }
    public string Download { get; set; }
}
