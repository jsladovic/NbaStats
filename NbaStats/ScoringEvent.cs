using HtmlAgilityPack;
using System;
using System.Text.RegularExpressions;

namespace NbaStats
{
    public class ScoringEvent : Event
    {
        public bool Made;
        public int Points;
        public string ShootingPlayer;

        public ScoringEvent(HtmlNodeCollection urlNodes, bool made, int points)
        {
            Made = made;
            Points = points;

            if (urlNodes.Count == 0)
                throw new Exception($"no player nodes in scoring event");
            ShootingPlayer = EventFactory.GetLinkFromUrlNode(urlNodes[0]);
        }
    }

    public class RegularShot : ScoringEvent
    {
        private string ShotPattern = "from (?<distance>[0-9]+) ft";

        public int Distance;
        public string AssistingPlayer;
        public string BlockingPlayer;

        public RegularShot(string text, HtmlNodeCollection urlNodes, bool made, int points) : base(urlNodes, made, points)
        {
            Regex pattern = new Regex(ShotPattern);
            System.Text.RegularExpressions.Match match = pattern.Match(text);
            if (match.Success)
            {
                string distanceString = match.Groups["distance"].Value;
                Distance = int.Parse(distanceString);
            }

            if (urlNodes.Count == 1)
                return;
            if (text.Contains("assist"))
                AssistingPlayer = EventFactory.GetLinkFromUrlNode(urlNodes[1]);
            else if (text.Contains("block"))
                BlockingPlayer = EventFactory.GetLinkFromUrlNode(urlNodes[1]);
            else
                throw new Exception($"unknown 2nd player role in shooting event");
        }

        public override string ToString()
        {
            return $@"{Quarter}/{TimeRemaining} - {Team} {Points} points, {(Distance > 0 ? $"{Distance}ft" : "at the rim")} {(Made ? "made" : "missed")}
    shot: {ShootingPlayer}, assist: {AssistingPlayer}, block: {BlockingPlayer}";
        }
    }

    public class FreeThrow : ScoringEvent
    {
        private string FreeThrowPattern = "(?<from>[0-9]) of (?<to>[0-9])";

        int FreeThrowNumber;
        int OutOfNumber;
        bool Technical;

        public FreeThrow(string text, HtmlNodeCollection urlNodes, bool made) : base(urlNodes, made, 1)
        {
            if (text.Contains("technical"))
            {
                Technical = true;
                return;
            }

            Regex pattern = new Regex(FreeThrowPattern);
            System.Text.RegularExpressions.Match match = pattern.Match(text);
            if (!match.Success)
                throw new Exception($"unable to parse free throw {text}");

            string tempString = match.Groups["from"].Value;
            FreeThrowNumber = int.Parse(tempString);

            tempString = match.Groups["to"].Value;
            OutOfNumber = int.Parse(tempString);
        }

        public override string ToString()
        {
            return $@"{Quarter}/{TimeRemaining} - {Team} {Points} points {(Made ? "made" : "missed")} 
    shot: {ShootingPlayer} {(Technical ? "technical" : $"{FreeThrowNumber} out of {OutOfNumber}")}";
        }
    }
}
