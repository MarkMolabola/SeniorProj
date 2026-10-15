using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;

namespace MDRNJ.Shared.DTOs.Challenge
{
    /// <summary>A 1v1 friend challenge, seen from the requesting user's side ("me" vs "them").</summary>
    public class ChallengeDto
    {
        public Guid ChallengeId { get; set; }
        public string Contest { get; set; } = null!;
        public string Scoring { get; set; } = null!;
        public float? DistanceKm { get; set; }

        /// <summary>Pending, Active, InReview, Completed, Disputed, Declined, Expired or Cancelled.</summary>
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime Deadline { get; set; }

        /// <summary>True when I issued the challenge.</summary>
        public bool IsChallenger { get; set; }
        public Guid OpponentUserId { get; set; }
        public string OpponentUsername { get; set; } = null!;

        public float? MyScore { get; set; }
        public float? TheirScore { get; set; }
        public int MyAttempts { get; set; }
        public int TheirAttempts { get; set; }

        /// <summary>"Won", "Lost" or "Tie" once completed.</summary>
        public string? Outcome { get; set; }
        public int XPEarned { get; set; }

        /// <summary>False until I've seen the result popup.</summary>
        public bool ResultSeen { get; set; }

        /// <summary>Every result needs a video (reps, plank) or photo (run).</summary>
        public bool RequireProof { get; set; }

        /// <summary>Results go through a review window where either player can dispute.</summary>
        public bool AllowDisputes { get; set; }

        /// <summary>"Video" or "Photo".</summary>
        public string ProofKind { get; set; } = string.Empty;

        /// <summary>Proof attached to results so far (both players).</summary>
        public List<ChallengeProofDto> Proofs { get; set; } = new();

        /// <summary>When the review closes and the result counts, while InReview.</summary>
        public DateTime? ReviewDeadline { get; set; }

        /// <summary>I can approve or dispute (in review, haven't approved yet).</summary>
        public bool CanReview { get; set; }
        public bool IApproved { get; set; }
        public bool TheyApproved { get; set; }

        /// <summary>Set when Disputed.</summary>
        public bool DisputedByMe { get; set; }
        public string? DisputeReason { get; set; }

        public bool CanRespond { get; set; }
        public bool CanCancel { get; set; }
        public bool CanSubmit { get; set; }

        /// <summary>Set when this request resolved the challenge.</summary>
        public CharacterProgressDto? CharacterProgress { get; set; }

        /// <summary>Set when this request resolved the challenge.</summary>
        public AccountProgressDto? AccountProgress { get; set; }
    }

    public class ChallengeProofDto
    {
        public Guid AttemptId { get; set; }
        public bool IsMine { get; set; }

        /// <summary>The result this proof backs (reps, seconds or km).</summary>
        public float Value { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
