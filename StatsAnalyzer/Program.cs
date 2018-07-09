using NbaStats;
using System;
using System.Collections.Generic;
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

            foreach (string fileName in fileNames)
            {
                StreamReader reader = new StreamReader(fileName);
                season.Matches.Add((Match)serializer.Deserialize(reader));
                reader.Close();
            }

            PlayerStats.Players = GetAllPlayerIds(season);

            PlayersWithMostPoints(season);
            PlayersWithMostUnassistedPoints(season);
            MostShotsDuringFinalMinutes(season, 2, true);
            MostShotsDuringFinalMinutes(season, 2, false);
            MostFreeThrowsDuringFinalMinutes(season, 2, true);
            MostFreeThrowsDuringFinalMinutes(season, 2, false);
            Console.ReadLine();
        }

        private static Dictionary<string, string> GetAllPlayerIds(Season season)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            List<Player> players = new List<Player>();
            foreach (Match match in season.Matches)
            {
                foreach (Player player in match.HomePlayers)
                {
                    if (!dic.Keys.Contains(player.Id))
                        dic[player.Id] = player.Name;
                }
                foreach (Player player in match.AwayPlayers)
                {
                    if (!dic.Keys.Contains(player.Id))
                        dic[player.Id] = player.Name;
                }
            }

            return dic;
        }

        private static void MostMissedShotsWithoutMakingAnyInFinalMInutes(Season season, int numberOfMinutes)
        {

        }

        private static void MostShotsDuringFinalMinutes(Season season, int numberOfMinutes, bool made)
        {
            var groupedPoints = season.AllEvents().Where(e => e is RegularShot && (e as RegularShot).Made == made && (e as RegularShot).Quarter == 4
            && (e as RegularShot).TimeRemaining <= 60 * numberOfMinutes).GroupBy(e => (e as RegularShot).ShootingPlayer);
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { Id = g.Key, Stat = g.Count() }).OrderByDescending(p => p.Stat).Take(10).ToList();
            Console.WriteLine($"\nMost {(made ? "made" : "missed")} shots during final {numberOfMinutes} minutes:");
            foreach (PlayerStats stat in stats)
                Console.WriteLine(stat);
        }

        private static void MostFreeThrowsDuringFinalMinutes(Season season, int numberOfMinutes, bool made)
        {
            var groupedPoints = season.AllEvents().Where(e => e is FreeThrow && (e as FreeThrow).Made == made && (e as FreeThrow).Quarter == 4
            && (e as FreeThrow).TimeRemaining <= 60 * numberOfMinutes).GroupBy(e => (e as FreeThrow).ShootingPlayer);
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { Id = g.Key, Stat = g.Count() }).OrderByDescending(p => p.Stat).Take(10).ToList();
            Console.WriteLine($"\nMost {(made ? "made" : "missed")} free throws during final {numberOfMinutes} minutes:");
            foreach (PlayerStats stat in stats)
                Console.WriteLine(stat);
        }

        private static void PlayersWithMostUnassistedPoints(Season season)
        {
            var groupedPoints = season.AllEvents().Where(e => e is RegularShot && (e as RegularShot).Made && (e as RegularShot).AssistingPlayer == null).
                GroupBy(e => (e as RegularShot).ShootingPlayer);
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { Id = g.Key, Stat = g.Sum(p => (p as RegularShot).Points) }).OrderByDescending(p => p.Stat).Take(10).ToList();
            Console.WriteLine("\nMost unassisted points during the regular season:");
            foreach (PlayerStats stat in stats)
                Console.WriteLine(stat);
        }

        private static void PlayersWithMostPoints(Season season)
        {
            var groupedPoints = season.AllEvents().Where(e => e is ScoringEvent && (e as ScoringEvent).Made).GroupBy(e => (e as ScoringEvent).ShootingPlayer);
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { Id = g.Key, Stat = g.Sum(p => (p as ScoringEvent).Points) }).OrderByDescending(p => p.Stat).Take(10).ToList();
            Console.WriteLine("\nMost points during the regular season:");
            foreach (PlayerStats stat in stats)
                Console.WriteLine(stat);
        }
    }

    public class PlayerStats
    {
        public static Dictionary<string, string> Players;

        public string Id;
        public int Stat;
        public int OtherStat;

        public override string ToString()
        {
            return $"{Players[Id]} - {(Stat == 0 && OtherStat != 0 ? OtherStat : Stat)}";
        }
    }
}
