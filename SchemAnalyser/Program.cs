using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace SchemAnalyser
{
    public class Program
    {

        public static Dictionary<string, int> ID_TO_SCORE = new Dictionary<string, int>();


        public static void Main(string[] args)
        {
            ID_TO_SCORE["copycats:copycat_board"] = 1;
            ID_TO_SCORE["vs_clockwork:flap_bearing"] = 2;
            ID_TO_SCORE["vs_clockwork:flap"] = 2;



            byte[] bytes = File.ReadAllBytes("abc.vschem");

            var stream = new MemoryStream(bytes);
            
            Schematic schematic = Schematic.FromStream(stream);
            Console.WriteLine(JsonSerializer.Serialize(schematic));
        }
    }
}