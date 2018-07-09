using System;
using System.Collections.Generic;
using System.Linq;

namespace NbaStats
{
    public class Match
    {
        public int HomeScore;
        public string HomeTeam;
        public List<Player> HomePlayers;

        public int AwayScore;
        public string AwayTeam;
        public List<Player> AwayPlayers;

        public List<Event> Events;
        public bool Playoffs;
        public DateTime Date;

        public Match() { }

        public Match(DateTime date, bool playoffs)
        {
            Playoffs = playoffs;
            Date = date;
            Events = new List<Event>();
        }

        public List<Event> ScoringEvents => Events.Where(e => e is ScoringEvent).ToList();

        public List<Event> MadeShots => ScoringEvents.Where(e => (e as ScoringEvent).Made).ToList();

        public int HomePoints => Events.Where(e => e.Team == EventTeam.Home && e is ScoringEvent && (e as ScoringEvent).Made).Sum(e => (e as ScoringEvent).Points);

        public int AwayPoints => Events.Where(e => e.Team == EventTeam.Away && e is ScoringEvent && (e as ScoringEvent).Made).Sum(e => (e as ScoringEvent).Points);

        public int PointsForPlayer(string id)
        {
            EventTeam? team = null;
            if (HomePlayers.Any(p => p.Id == id))
                team = EventTeam.Home;
            else if (AwayPlayers.Any(p => p.Id == id))
                team = EventTeam.Away;
            else
                throw new Exception($"Player {id} doesn't appear in any of the teams");

            return Events.Where(e => e.Team == team && e is ScoringEvent && (e as ScoringEvent).Made && (e as ScoringEvent).ShootingPlayer == id).
                Sum(e => (e as ScoringEvent).Points);
        }

        public bool CheckScore()
        {
            if (HomeScore != HomePoints || AwayScore != AwayPoints)
                return false;

            return true;
        }

        public string CheckPlayerScores()
        {
            foreach (Player player in HomePlayers)
                if (player.Points != PointsForPlayer(player.Id))
                    return player.Id;

            foreach (Player player in AwayPlayers)
                if (player.Points != PointsForPlayer(player.Id))
                    return player.Id;

            return null;
        }

        public override string ToString()
        {
            return $"{Date}{(Playoffs ? " PO " : "")} {HomeTeam} {HomePoints}:{AwayPoints} {AwayTeam}";
        }
    }

    public class Player
    {
        public string Name;
        public string Id;

        public int Points;
        public bool DidNotPlay;

        public Player() { }

        public Player(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            if (DidNotPlay)
                return $"{Name} did not play\t{Id}";

            return $"{Name}\t{Points} points\t{Id}";
        }
    }
}
