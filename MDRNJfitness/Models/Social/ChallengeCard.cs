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
            Proofs = dto.Proofs.Select(p => new ProofItem(
                dto.ChallengeId, p.AttemptId,
                $"{(p.IsMine ? "YOU" : dto.OpponentUsername.ToUpperInvariant())} · {DuelRules.Format(p.Value, Contest, Scoring)}",
                p.IsMine)).ToList();
        }

        // ── Proof ───────────────────────────────────────────────────────────

        public ProofKind ProofKind => DuelRules.ProofKindFor(Contest);
        public IReadOnlyList<ProofItem> Proofs { get; }
        public bool HasProofs => Proofs.Count > 0;

        public string ProofRuleText => !Dto.RequireProof ? string.Empty
            : Dto.AllowDisputes ? $"PROOF REQUIRED ({(ProofKind == ProofKind.Video ? "VIDEO" : "PHOTO")}) · REVIEWED"
            : $"PROOF REQUIRED ({(ProofKind == ProofKind.Video ? "VIDEO" : "PHOTO")})";

        public bool IsInReview => Dto.Status == "InReview";

        /// <summary>Review panel text while InReview.</summary>
        public string ReviewText => !IsInReview ? string.Empty
            : Dto.IApproved
                ? $"You approved. Waiting for {Dto.OpponentUsername}; the result counts in {Left(Dto.ReviewDeadline)} if nobody disputes."
                : $"Check {Dto.OpponentUsername}'s proof. APPROVE if it holds up, DISPUTE to void the duel. Counts in {Left(Dto.ReviewDeadline)}.";

        public string DisputeText => Dto.Status != "Disputed" ? string.Empty
            : (Dto.DisputedByMe ? "You disputed the proof." : $"{Dto.OpponentUsername} disputed your proof.")
              + " Void: no winner, no rewards."
              + (string.IsNullOrEmpty(Dto.DisputeReason) ? "" : $" Reason: \"{Dto.DisputeReason}\"");
        public bool IsDisputed => Dto.Status == "Disputed";

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
            "InReview" => "IN REVIEW",
            "Disputed" => "VOID · DISPUTED",
            "Completed" => Dto.Outcome switch
            {
                "Won" => $"★ VICTORY  +{Dto.XPEarned} XP",
                "Tie" => $"DRAW  +{Dto.XPEarned} XP",
                _ => Dto.XPEarned > 0 ? $"DEFEAT  +{Dto.XPEarned} XP" : "DEFEAT"
            },
            _ => Dto.Status.ToUpperInvariant()
        };

        public bool IsWon => Dto.Outcome == "Won";
        public bool IsFinished => Dto.Status is not ("Pending" or "Active" or "InReview");
        public bool ShowScores => Dto.Status is "Active" or "InReview" or "Completed" or "Disputed";

        public string SubmitText => Dto.RequireProof ? (ProofKind == ProofKind.Video ? "RECORD + SUBMIT" : "ADD PHOTO + SUBMIT")
            : DuelRules.UsesStopwatch(Contest, Scoring) ? "START TIMER"
            : Scoring == DuelScoring.Total ? "ADD RESULT"
            : "LOCK IN RESULT";

        private string Score(float? value) =>
            value is float v ? DuelRules.Format(v, Contest, Scoring) : "—";

        private static string Left(DateTime? until)
        {
            var left = (until ?? DateTime.UtcNow) - DateTime.UtcNow;
            return left.TotalHours >= 1 ? $"{(int)left.TotalHours}h {left.Minutes}m" : $"{Math.Max(1, (int)left.TotalMinutes)}m";
        }

        private string TimeLeft()
        {
            var left = Dto.Deadline - DateTime.UtcNow;
            if (left <= TimeSpan.Zero) return "ENDED";
            return left.TotalHours >= 24 ? $"{(int)left.TotalDays}D {left.Hours}H LEFT"
                : left.TotalMinutes >= 60 ? $"{(int)left.TotalHours}H {left.Minutes}M LEFT"
                : $"{Math.Max(1, (int)left.TotalMinutes)}M LEFT";
        }
    }

    /// <summary>One proof on a challenge card (tap VIEW to open it).</summary>
    public record ProofItem(Guid ChallengeId, Guid AttemptId, string Label, bool IsMine);
}
