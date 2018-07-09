
using System.Xml.Serialization;

namespace NbaStats
{
    [XmlInclude(typeof(ScoringEvent))]
    public class Event
    {
        public EventTeam Team;
        public EventType Type;
        public int Quarter;
        public float TimeRemaining;

        public void SetEventDetails(EventTeam team, EventType type,  int quarter, float timeRemaining)
        {
            Team = team;
            Type = type;
            Quarter = quarter;
            TimeRemaining = timeRemaining;
        }

        public override string ToString()
        {
            return $"{Quarter}/{TimeRemaining} - {Team} - {Type}";
        }
    }

    public enum EventTeam
    {
        Neutral,
        Home,
        Away
    }

    public enum EventType
    {
        PeriodStart,
        JumpBall,
        MakesFreeThrow,
        MissesFreeThrow,
        Makes2pt,
        Misses2pt,
        Makes3pt,
        Misses3pt,
        DefensiveRebound,
        OffensiveRebound,
        Foul,
        Timeout,
        Substitution,
        Turnover,
        Violation,
        PeriodEnd,
    }

    public enum FoulType
    {
        Personal,
        LooseBall,
        Shooting,
        Technical,
        Charge,
        Def3sec,
        Off3sec,
    }
}
