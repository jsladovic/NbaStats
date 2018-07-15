
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

        public bool FinalMinutes(int numberOfMinutes, bool includeOvertime = false)
        {
            if (Quarter == 4 && TimeRemaining <= 60 * numberOfMinutes)
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"{Quarter}/{TimeRemaining} - {Team} - {Type}";
        }

        public bool IsMadeShot => (this is ScoringEvent) && (this as ScoringEvent).Made;

        public int QuarterMinutes => Quarter <= 4 ? 12 : 5;

        public int MinuteOfEvent => (int)((Quarter - 1) * 12 + (QuarterMinutes * 60 - TimeRemaining) / 60) + (TimeRemaining == 0.0 ? 0 : 1);
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
