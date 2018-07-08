using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NbaStats
{
    class SeasonParser
    {
        private const string MatchesUrl = "https://www.basketball-reference.com/leagues/NBA_{0}_games-{1}.html";
        private static readonly string[] Months = new string[] { "october", "november", "december", "january", "february", "march", "april", "may", "june"};

        private const string MatchesTableXpath = "//*[@id=\"schedule\"]/tbody";
        private const string UrlRegex = "\\/boxscores\\/(?<id>[a-zA-Z0-9]+)\\.html";

        public static Season ParseSeason(string year)
        {
            Season season = new Season(year);
            bool playoffs = false;
            foreach (string month in Months)
            {
                Console.WriteLine($"{month}, {year}");
                season.Matches.AddRange(ParseMonth(string.Format(MatchesUrl, year, month), ref playoffs));
            }


            return season;
        }

        private static List<Match> ParseMonth(string url, ref bool playoffs)
        {
            List<Match> matches = new List<Match>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNode tableNode = doc.DocumentNode.SelectSingleNode(MatchesTableXpath);

            foreach (HtmlNode matchNode in tableNode.ChildNodes)
            {
                if (matchNode.ChildNodes.Count == 0)
                    continue;

                if (matchNode.ChildNodes.Count == 1 && matchNode.ChildNodes[0].InnerText == "Playoffs")
                {
                    playoffs = true;
                    Console.WriteLine("start of playoffs");
                    continue;
                }


                // todo check for playoffs th
                Match match = ParseMatch(matchNode, playoffs);
                Console.WriteLine(match);
                matches.Add(match);
            }

            return matches;
        }

        private static Match ParseMatch(HtmlNode node, bool playoffs)
        {
            Regex pattern = new Regex(UrlRegex);
            System.Text.RegularExpressions.Match match = pattern.Match(EventFactory.GetLinkFromUrlNode(node.ChildNodes[6].SelectSingleNode("a")));
            if (!match.Success)
                throw new Exception($"unable to parse match id {node.ChildNodes[6].InnerHtml}");

            string id = match.Groups["id"].Value;
            return MatchParser.ParseMatch(id, ParseDateTime(node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText), playoffs);
        }

        private static DateTime ParseDateTime(string date, string time)
        {
            return DateTime.Parse($"{date} {time}");
        }
    }

    class Season
    {
        public string Year;
        public List<Match> Matches;

        public Season(string year)
        {
            Year = year;
            Matches = new List<Match>();
        }
    }
}
