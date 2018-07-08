using HtmlAgilityPack;
using System;

namespace NbaStats
{
    public class EventFactory
    {
        private const string EmptyString = "&nbsp;";

        // todo delete
        //public static int homePoints = 0;
        //public static int awayPoints = 0;

        public static Event ParseEvent(HtmlNode node, int quarter)
        {
            HtmlNodeCollection nodes = node.SelectNodes("td");
            if (nodes.Count < 2)
                return null;

            float eventTime = EventTime(nodes[0].InnerText);
            EventTeam team = GetEventTeam(nodes);

            HtmlNode detailsNode = team == EventTeam.Home ? nodes[5] : nodes[1];

            HtmlNodeCollection urlNodes;
            string text = GetTextAndRemoveUrlNodes(node, out urlNodes);

            EventType type = GetEventType(text);


            Event matchEvent = GetEvent(text, urlNodes, type);
            matchEvent.SetEventDetails(team, type, quarter, eventTime);
            /* todo delete
             * if (matchEvent is ScoringEvent && (matchEvent as ScoringEvent).Made)
            {
                if (team == EventTeam.Home)
                    homePoints += (matchEvent as ScoringEvent).Points;
                else if (team == EventTeam.Away)
                    awayPoints += (matchEvent as ScoringEvent).Points;
                Console.WriteLine($"{homePoints}:{awayPoints}");
                Console.ReadLine();
            }*/
            return matchEvent;
        }

        public static Event GetEvent(string text, HtmlNodeCollection urlNodes, EventType type)
        {
            switch (type)
            {
                case EventType.MakesFreeThrow:
                    return new FreeThrow(text, urlNodes, true);
                case EventType.Makes2pt:
                    return new RegularShot(text, urlNodes, true, 2);
                case EventType.Makes3pt:
                    return new RegularShot(text, urlNodes, true, 3);
                case EventType.MissesFreeThrow:
                    return new FreeThrow(text, urlNodes, false);
                case EventType.Misses2pt:
                    return new RegularShot(text, urlNodes, false, 2);
                case EventType.Misses3pt:
                    return new RegularShot(text, urlNodes, false, 3);
            }

            return new Event();
        }

        private static EventType GetEventType(string text)
        {
            if (text.Contains("start of"))
                return EventType.PeriodStart;
            if (text.Contains("end of"))
                return EventType.PeriodEnd;
            if (text.Contains("jump ball"))
                return EventType.JumpBall;
            if (text.Contains("timeout"))
                return EventType.Timeout;
            if (text.Contains("free throw"))
            {
                if (text.Contains("makes"))
                    return EventType.MakesFreeThrow;
                if (text.Contains("misses"))
                    return EventType.MissesFreeThrow;
            }
            if (text.Contains("2-pt shot"))
            {
                if (text.Contains("makes"))
                    return EventType.Makes2pt;
                if (text.Contains("misses"))
                    return EventType.Misses2pt;
            }
            if (text.Contains("3-pt shot"))
            {
                if (text.Contains("makes"))
                    return EventType.Makes3pt;
                if (text.Contains("misses"))
                    return EventType.Misses3pt;
            }
            if (text.Contains("offensive rebound"))
                return EventType.OffensiveRebound;
            if (text.Contains("defensive rebound"))
                return EventType.DefensiveRebound;
            if (text.Contains("enters"))
                return EventType.Substitution;
            if (text.Contains("turnover"))
                return EventType.Turnover;
            if (text.Contains("foul"))
                return EventType.Foul;
            if (text.Contains("violation"))
                return EventType.Violation;

            throw new Exception($"unknown event type: {text}");
        }

        private static EventTeam GetEventTeam(HtmlNodeCollection nodes)
        {
            if (nodes.Count < 6)
                return EventTeam.Neutral;

            if (nodes[1].InnerText != EmptyString && nodes[5].InnerText == EmptyString)
                return EventTeam.Away;

            if (nodes[1].InnerText == EmptyString && nodes[5].InnerText != EmptyString)
                return EventTeam.Home;

            throw new Exception($"unknown event team {nodes[1].InnerText} - {nodes[5].InnerText}");
        }

        private static float EventTime(string timeString)
        {
            string[] temp = timeString.Split(':');
            int minutes = int.Parse(temp[0]);
            float seconds = float.Parse(temp[1]);

            return 60 * minutes + seconds;
        }

        public static string GetTextAndRemoveUrlNodes(HtmlNode node, out HtmlNodeCollection urlNodes)
        {
            urlNodes = node.SelectNodes("td/a");
            if (urlNodes != null)
                foreach (HtmlNode urlNode in urlNodes)
                    urlNode.Remove();

            return node.InnerText.ToLower();
        }

        public static string GetLinkFromUrlNode(HtmlNode node)
        {
            string href = node.GetAttributeValue("href", null);
            if (href == null)
                throw new Exception($"unable to find url");

            return href;
        }
    }
}
