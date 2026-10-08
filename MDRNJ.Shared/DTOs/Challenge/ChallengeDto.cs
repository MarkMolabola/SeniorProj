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

        /// <summary>Pending, Active, Completed, Declined, Expired or Cancelled.</summary>
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

        public bool CanRespond { get; set; }
        public bool CanCancel { get; set; }
        public bool CanSubmit { get; set; }

        /// <summary>Set when this request resolved the challenge.</summary>
        public CharacterProgressDto? CharacterProgress { get; set; }

        /// <summary>Set when this request resolved the challenge.</summary>
        public AccountProgressDto? AccountProgress { get; set; }
    }
}
