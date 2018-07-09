using NbaStats;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace StatsAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string year = "2018";
            string cachePath = $"cache/{year}/";

            Season season = new Season(year);
            string[] fileNames = Directory.GetFiles(cachePath);
            XmlSerializer serializer = new XmlSerializer(typeof(Match));
            Console.WriteLine(fileNames[0]);

            foreach (string fileName in fileNames)
            {
                StreamReader reader = new StreamReader(fileName);
                season.Matches.Add((Match)serializer.Deserialize(reader));
                reader.Close();
            }

            Console.WriteLine(season.Matches.Count);
            Console.WriteLine(season.Matches.Where(m => !m.Playoffs).Count());
            Console.ReadLine();
        }
    }
}
