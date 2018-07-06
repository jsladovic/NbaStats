using System;
using System.Linq;

namespace NbaStats
{
    class Program
    {
        static void Main(string[] args)
        {
            Season season = SeasonParser.ParseSeason("2018");
            //Match m = MatchParser.ParseMatch("201710210UTA", DateTime.Today, false);

            Console.ReadLine();
        }
    }
}
