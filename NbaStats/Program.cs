using System;
using System.Linq;

namespace NbaStats
{
    class Program
    {
        static void Main(string[] args)
        {
            Match match = MatchParser.ParseMatch("201710170CLE");
            Console.WriteLine(match);

            Console.ReadLine();
        }
    }
}
