using MDRNJ.Shared.DTOs.Challenge;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;

namespace MDRNJfitness.Models.Social
{
    /// <summary>Display text for one challenge on the Challenges page.</summary>
    public class ChallengeCard
    {
        public ChallengeDto Dto { get; }
        public DuelContest Contest { get; }
        public DuelScoring Scoring { get; }

        public ChallengeCard(ChallengeDto dto)
        {
            Dto = dto;
            Contest = Enum.Parse<DuelContest>(dto.Contest);
            Scoring = Enum.Parse<DuelScoring>(dto.Scoring);
        }

        public string Title => DuelRules.IsTimedRace(Contest, Scoring) && Dto.DistanceKm is float km
            ? $"{km:0.##} KM RACE"
            : DuelRules.ContestName(Contest).ToUpperInvariant();

        public string ModeText => DuelRules.IsTimedRace(Contest, Scoring) ? "Fastest time wins"
            : Scoring == DuelScoring.BestAttempt ? "Best single attempt"
            : $"Most total {DuelRules.Unit(Contest, Scoring)} by the deadline";

        public string VersusText => Dto.IsChallenger
            ? $"You challenged {Dto.OpponentUsername}"
            : $"{Dto.OpponentUsername} challenged you";

        public string MyScoreText => Score(Dto.MyScore);
        public string TheirScoreText => Score(Dto.TheirScore);

        public string StatusText => Dto.Status switch
        {
            "Pending" when Dto.CanRespond => "AWAITING YOUR ANSWER",
            "Pending" => "WAITING FOR REPLY",
            "Active" => TimeLeft(),
            "Completed" => Dto.Outcome switch
            {
                "Won" => $"★ VICTORY  +{Dto.XPEarned} XP",
                "Tie" => $"DRAW  +{Dto.XPEarned} XP",
                _ => Dto.XPEarned > 0 ? $"DEFEAT  +{Dto.XPEarned} XP" : "DEFEAT"
            },
            _ => Dto.Status.ToUpperInvariant()
        };

        public bool IsWon => Dto.Outcome == "Won";
        public bool IsFinished => Dto.Status is not ("Pending" or "Active");
        public bool ShowScores => Dto.Status is "Active" or "Completed";

        public string SubmitText => DuelRules.UsesStopwatch(Contest, Scoring) ? "START TIMER"
            : Scoring == DuelScoring.Total ? "ADD RESULT"
            : "LOCK IN RESULT";

        private string Score(float? value) =>
            value is float v ? DuelRules.Format(v, Contest, Scoring) : "—";

        private string TimeLeft()
        {
            var left = Dto.Deadline - DateTime.UtcNow;
            if (left <= TimeSpan.Zero) return "ENDED";
            return left.TotalHours >= 24 ? $"{(int)left.TotalDays}D {left.Hours}H LEFT"
                : left.TotalMinutes >= 60 ? $"{(int)left.TotalHours}H {left.Minutes}M LEFT"
                : $"{Math.Max(1, (int)left.TotalMinutes)}M LEFT";
        }
    }
}
