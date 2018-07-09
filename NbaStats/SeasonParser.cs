using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace NbaStats
{
    class SeasonParser
    {
        public static string CachePath;

        private const string MatchesUrl = "https://www.basketball-reference.com/leagues/NBA_{0}_games-{1}.html";
        private static readonly string[] Months = new string[] { "october", "november", "december", "january", "february", "march", "april", "may", "june" };

        private const string MatchesTableXpath = "//*[@id=\"schedule\"]/tbody";
        private const string UrlRegex = "\\/boxscores\\/(?<id>[a-zA-Z0-9]+)\\.html";

        public static Season ParseSeason(string year)
        {
            CachePath = $"cache/{year}/";
            Directory.CreateDirectory(CachePath);
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
                if (match == null)
                    continue;

                Console.WriteLine(match);
                matches.Add(match);
            }

            return matches;
        }

        private static Match ParseMatch(HtmlNode node, bool playoffs)
        {
            Regex pattern = new Regex(UrlRegex);
            System.Text.RegularExpressions.Match regexMatch = pattern.Match(EventFactory.GetLinkFromUrlNode(node.ChildNodes[6].SelectSingleNode("a")));
            if (!regexMatch.Success)
                throw new Exception($"unable to parse match id {node.ChildNodes[6].InnerHtml}");

            string id = regexMatch.Groups["id"].Value;
            if (CachedMatchExists(id))
                return null;

            Match match = MatchParser.ParseMatch(id, ParseDateTime(node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText), playoffs);
            CacheMatch(match, id);
            return match;
        }

        private static void CacheMatch(Match match, string id)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Match));
                string xml;
                using (var sww = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sww))
                    {
                        xmlSerializer.Serialize(writer, match);
                        xml = sww.ToString(); // Your XML
                    }
                }
                File.WriteAllText(GetCacheFileName(id), xml);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static bool CachedMatchExists(string id)
        {
            if (File.Exists(GetCacheFileName(id)))
                return true;

            return false;
        }

        private static DateTime ParseDateTime(string date, string time)
        {
            return DateTime.Parse($"{date} {time}");
        }

        private static string GetCacheFileName(string id)
        {
            return $"{CachePath}{id}.xml";
        }
    }

    public class Season
    {
        public string Year;
        public List<Match> Matches;

        public Season(string year)
        {
            Year = year;
            Matches = new List<Match>();
        }

        public List<Match> RegularSeasonMatches => Matches.Where(m => !m.Playoffs).ToList();
    }
}
