using NbaStats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
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

            List<PlayerStats> topScorers = GetTopScorers(season, 20);
            foreach (PlayerStats topScorer in topScorers)
                PrintPlayerShotChart(season, topScorer.PlayerId);

            #region Printing player stats

            //PlayersWithMostPoints(season);
            //PlayersWithMostUnassistedPoints(season);
            //MostShotsDuringFinalMinutes(season, 2, true);
            //MostShotsDuringFinalMinutes(season, 2, false);
            //MostFreeThrowsDuringFinalMinutes(season, 2, true);
            //MostFreeThrowsDuringFinalMinutes(season, 2, false);
            //MostMissedShotsWithoutMakingAnyInFinalMInutes(season, 2);
            //BestAssistScoreCombination(season);
            //BestAssistScoreCombination(season, true);
            //BestDuo(season);
            //BestDuo(season, true);
            //BlockedTheMost(season);

            #endregion

            Console.ReadLine();
        }

        private static void PrintPlayerShotChart(Season season, string playerId)
        {
            List<Event> madeShotsForPlayer = season.AllEvents().Where(e => e.IsMadeShot && (e as ScoringEvent).ShootingPlayer == playerId && e.Quarter <= 4).ToList();
            var shotsGroupedByMinutes = madeShotsForPlayer.GroupBy(e => e.MinuteOfEvent).OrderBy(g => g.Key).ToList();

            Console.WriteLine(PlayerStats.Players[playerId]);
            Chart chart = new Chart();
            chart.Size = new Size(1024, 512);
            chart.Palette = ChartColorPalette.SeaGreen;
            chart.Titles.Add(PlayerStats.Players[playerId]);

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisX.Interval = 12;
            chartArea.AxisY.Maximum = 120;
            chart.ChartAreas.Add(chartArea);
            Series series = new Series();

            foreach (var minutes in shotsGroupedByMinutes)
                series.Points.AddXY(minutes.Key, minutes.Sum(e => (e as ScoringEvent).Points));
                
            foreach (var point in series.Points)
                if (point.XValue <= 12)
                    point.Color = Color.Blue;
                else if (point.XValue <= 24)
                    point.Color = Color.Pink;
                else if (point.XValue <= 36)
                    point.Color = Color.Green;
                else
                    point.Color = Color.Red;

            chart.Series.Add(series);
            chart.Invalidate();
            chart.SaveImage($"./{PlayerStats.Players[playerId].Replace(' ', '_')}.png", ChartImageFormat.Png);
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

        #region Printing player stats methods

        private static void PrintStats(List<PlayerStats> stats, string text)
        {
            Console.WriteLine(text);
            foreach (PlayerStats stat in stats)
                Console.WriteLine(stat);
        }

        private static List<PlayerStats> GetTopScorers(Season season, int numberOfPlayers)
        {
            var groupedPoints = season.AllEvents().Where(e => e is ScoringEvent && (e as ScoringEvent).Made).GroupBy(e => (e as ScoringEvent).ShootingPlayer);
            return groupedPoints.Select(g => new PlayerStats
            { PlayerId = g.Key, Stat = g.Sum(p => (p as ScoringEvent).Points) }).OrderByDescending(p => p.Stat).Take(numberOfPlayers).ToList();
        }

        private static void BlockedTheMost(Season season)
        {
            List<PlayerStats> stats = season.AllEvents().Where(e => e is RegularShot && !(e as RegularShot).Made && (e as RegularShot).BlockingPlayer != null).
                GroupBy(e => (e as RegularShot).ShootingPlayer).Select(g => new PlayerStats { PlayerId = g.Key, Stat = g.Count() }).
                OrderByDescending(p => p.Stat).Take(10).ToList();
            PrintStats(stats, "\nPlayers that have been blocked the most:");
        }

        private static void BestDuo(Season season, bool countPoints = false)
        {
            var groupedPoints = season.AllEvents().Where(e => e is RegularShot && (e as RegularShot).Made && (e as RegularShot).AssistingPlayer != null).
                GroupBy(e => new
                {
                    FirstPlayer = string.Compare((e as RegularShot).AssistingPlayer, (e as RegularShot).ShootingPlayer) < 0 ?
                (e as RegularShot).AssistingPlayer : (e as RegularShot).ShootingPlayer,
                    SecondPlayer = string.Compare((e as RegularShot).AssistingPlayer, (e as RegularShot).ShootingPlayer) < 0 ?
                    (e as RegularShot).ShootingPlayer : (e as RegularShot).AssistingPlayer,
                });
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats
                {
                    PlayerId = g.Key.FirstPlayer,
                    Player2Id = g.Key.SecondPlayer,
                    Stat = countPoints ? g.Sum(p => (p as RegularShot).Points) : g.Count()
                }).OrderByDescending(p => p.Stat).Take(10).ToList();
            PrintStats(stats, $"\nBest duo {(countPoints ? "(number of points)" : "(number of made shots)")}:");

        }

        private static void BestAssistScoreCombination(Season season, bool countPoints = false)
        {
            var groupedPoints = season.AllEvents().Where(e => e is RegularShot && (e as RegularShot).Made && (e as RegularShot).AssistingPlayer != null).
                GroupBy(e => new { (e as RegularShot).ShootingPlayer, (e as RegularShot).AssistingPlayer });
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats
                {
                    PlayerId = g.Key.AssistingPlayer,
                    Player2Id = g.Key.ShootingPlayer,
                    Stat = countPoints ? g.Sum(p => (p as RegularShot).Points) : g.Count()
                }).OrderByDescending(p => p.Stat).Take(10).ToList();
            PrintStats(stats, $"\nBest assist -> score combinations {(countPoints ? "(number of points)" : "(number of made shots)")}:");
        }

        private static void MostMissedShotsWithoutMakingAnyInFinalMInutes(Season season, int numberOfMinutes)
        {
            List<Event> allEvents = season.AllEvents();
            var groupedPoints = allEvents.Where(e => e is RegularShot && !(e as RegularShot).Made && e.FinalMinutes(numberOfMinutes)).
                GroupBy(e => (e as RegularShot).ShootingPlayer).Where(g => !allEvents.Any(e => e is RegularShot && (e as RegularShot).Made
                && e.FinalMinutes(numberOfMinutes) && (e as RegularShot).ShootingPlayer == g.Key));
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { PlayerId = g.Key, Stat = g.Count() }).OrderByDescending(p => p.Stat).Take(10).ToList();
            PrintStats(stats, $"\nMost missed shots in final {numberOfMinutes} minutes without making any:");
        }

        private static void MostShotsDuringFinalMinutes(Season season, int numberOfMinutes, bool made)
        {
            var groupedPoints = season.AllEvents().Where(e => e is RegularShot && (e as RegularShot).Made == made && e.FinalMinutes(numberOfMinutes)).
                GroupBy(e => (e as RegularShot).ShootingPlayer);
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { PlayerId = g.Key, Stat = g.Count() }).OrderByDescending(p => p.Stat).Take(10).ToList();
            PrintStats(stats, $"\nMost {(made ? "made" : "missed")} shots during final {numberOfMinutes} minutes:");
        }

        private static void MostFreeThrowsDuringFinalMinutes(Season season, int numberOfMinutes, bool made)
        {
            var groupedPoints = season.AllEvents().Where(e => e is FreeThrow && (e as FreeThrow).Made == made && e.FinalMinutes(numberOfMinutes)).
                GroupBy(e => (e as FreeThrow).ShootingPlayer);
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { PlayerId = g.Key, Stat = g.Count() }).OrderByDescending(p => p.Stat).Take(10).ToList();
            PrintStats(stats, $"\nMost {(made ? "made" : "missed")} free throws during final {numberOfMinutes} minutes:");
        }

        private static void PlayersWithMostUnassistedPoints(Season season)
        {
            var groupedPoints = season.AllEvents().Where(e => e is RegularShot && (e as RegularShot).Made && (e as RegularShot).AssistingPlayer == null).
                GroupBy(e => (e as RegularShot).ShootingPlayer);
            List<PlayerStats> stats = groupedPoints.
                Select(g => new PlayerStats { PlayerId = g.Key, Stat = g.Sum(p => (p as RegularShot).Points) }).OrderByDescending(p => p.Stat).Take(10).ToList();
            PrintStats(stats, "\nMost unassisted points during the regular season:");
        }

        private static void PlayersWithMostPoints(Season season)
        {
            PrintStats(GetTopScorers(season, 10), "\nMost points during the regular season:");
        }

        #endregion
    }

    public class PlayerStats
    {
        public static Dictionary<string, string> Players;

        public string PlayerId;
        public string Player2Id;
        public int Stat;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Player2Id))
                return $"{Players[PlayerId]} - {Stat}";

            return $"{Players[PlayerId]} - {Players[Player2Id]} - {Stat}";
        }
    }
}