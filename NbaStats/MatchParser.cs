using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace NbaStats
{
    class MatchParser
    {
        private const string boxScoreUrl = "https://www.basketball-reference.com/boxscores/{0}.html";
        private const string playByPlayUrl = "https://www.basketball-reference.com/boxscores/pbp/{0}.html";

        #region X-path variables
        private const string pbpNodesXpath = "//*[@id=\"pbp\"]/tr";
        private const string statsTablesXpath = "//table[contains(@class, \"stats_table\")]/tbody";
        private const string awayScoreXpath = "//*[@id=\"content\"]/div[2]/div[1]/div[2]";
        private const string homeScoreXpath = "//*[@id=\"content\"]/div[2]/div[2]/div[2]";
        private const string awayNameXpath = "//*[@id=\"content\"]/div[2]/div[1]/div[1]/strong/a";
        private const string homeNameXpath = "//*[@id=\"content\"]/div[2]/div[2]/div[1]/strong/a";
        #endregion

        public static Match ParseMatch(string matchId, DateTime date, bool playoffs)
        {
            HtmlWeb web = new HtmlWeb();
            Match match = ParseMatchBoxScore(web, matchId, date, playoffs);
            match.Events = ParseMatchPlayByPlay(web, matchId);
            if (!match.CheckScore())
                throw new Exception($"incorrect match score {match.HomePoints}:{match.AwayPoints} instead of {match.HomeScore}:{match.AwayScore}");
            string invalidPlayerId = match.CheckPlayerScores();
            if (invalidPlayerId != null)
                throw new Exception($"invalid player score for player {invalidPlayerId}");

            return match;
        }

        private static Match ParseMatchBoxScore(HtmlWeb web, string matchId, DateTime date, bool playoffs)
        {
            Match match = new Match(date, playoffs);
            string bsUrl = string.Format(boxScoreUrl, matchId);

            HtmlDocument doc = web.Load(bsUrl);
            ParseMatchTeamNamesAndScore(doc, match);
            
            HtmlNodeCollection statsTables = doc.DocumentNode.SelectNodes(statsTablesXpath);
            match.HomePlayers = ParseMatchBoxScore(statsTables[2]);
            match.AwayPlayers = ParseMatchBoxScore(statsTables[0]);

            return match;
        }

        private static void ParseMatchTeamNamesAndScore(HtmlDocument doc, Match match)
        {
            HtmlNode nameNode = doc.DocumentNode.SelectSingleNode(homeNameXpath);
            match.HomeTeam = nameNode.InnerText;

            nameNode = doc.DocumentNode.SelectSingleNode(awayNameXpath);
            match.AwayTeam = nameNode.InnerText;

            HtmlNode scoreNode = doc.DocumentNode.SelectSingleNode(homeScoreXpath);
            match.HomeScore = int.Parse(scoreNode.InnerText);

            scoreNode = doc.DocumentNode.SelectSingleNode(awayScoreXpath);
            match.AwayScore = int.Parse(scoreNode.InnerText);
        }

        private static List<Player> ParseMatchBoxScore(HtmlNode tableNode)
        {
            List<Player> players = new List<Player>();

            foreach (HtmlNode node in tableNode.ChildNodes)
            {
                if (node.ChildNodes.Count == 0 || node.HasClass("thead"))
                    continue;

                Player player = new Player(node.ChildNodes[0].InnerText);
                player.Id = EventFactory.GetLinkFromUrlNode(node.ChildNodes[0].SelectSingleNode("a"));
                players.Add(player);

                if (node.ChildNodes.Count == 2)
                {
                    switch(node.ChildNodes[1].InnerText)
                    {
                        case "Did Not Play":
                        case "Did Not Dress":
                        case "Player Suspended":
                        case "Not With Team":
                            player.DidNotPlay = true;
                            continue;
                        default:
                            throw new Exception($"could not parse inactive player {node.ChildNodes[1].InnerText}");
                    }
                }

                player.Points = int.Parse(node.ChildNodes[19].InnerText);
            }

            return players;
        }

        private static List<Event> ParseMatchPlayByPlay(HtmlWeb web, string matchId)
        {
            int currentQuarter = 0;
            string pbpUrl = string.Format(playByPlayUrl, matchId);
            List<Event> events = new List<Event>();

            HtmlDocument doc = web.Load(pbpUrl);
            HtmlNodeCollection pbpNodes = doc.DocumentNode.SelectNodes(pbpNodesXpath);

            foreach (HtmlNode node in pbpNodes)
            {
                string id = node.GetAttributeValue("id", null);
                if (id != null && id.Contains("q"))
                    currentQuarter++;

                if (node.HasClass("thead"))
                    continue;

                events.Add(EventFactory.ParseEvent(node, currentQuarter));
            }

            return events;
        }
    }
}
