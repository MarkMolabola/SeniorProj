using MDRNJ.Shared.Enums;
using System;
using System.Collections.Generic;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>A 1v1 contest one friend issues to another.</summary>
    public class ChallengeEntity
    {
        public Guid ChallengeId { get; set; } = Guid.NewGuid();
        public Guid ChallengerId { get; set; }
        public Guid OpponentId { get; set; }

        public DuelContest Contest { get; set; }
        public DuelScoring Scoring { get; set; }
        public float? DistanceKm { get; set; }

        public ChallengeStatus Status { get; set; } = ChallengeStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime Deadline { get; set; }
        public DateTime? ResolvedAt { get; set; }

        /// <summary>Best or total so far, per <see cref="Scoring"/>; null until a first attempt.</summary>
        public float? ChallengerScore { get; set; }
        public float? OpponentScore { get; set; }

        /// <summary>Null on a tie or before resolution.</summary>
        public Guid? WinnerId { get; set; }
        public int ChallengerXP { get; set; }
        public int OpponentXP { get; set; }
        public bool ChallengerSeenResult { get; set; }
        public bool OpponentSeenResult { get; set; }

        /// <summary>Every result needs a proof video / photo.</summary>
        public bool RequireProof { get; set; }

        /// <summary>Results go to a review window (InReview) where either player can dispute.</summary>
        public bool AllowDisputes { get; set; }

        /// <summary>While InReview: when the result counts if nobody disputes.</summary>
        public DateTime? ReviewDeadline { get; set; }
        public bool ChallengerApproved { get; set; }
        public bool OpponentApproved { get; set; }

        /// <summary>Set when Disputed.</summary>
        public Guid? DisputedById { get; set; }
        public string? DisputeReason { get; set; }

        // Navigation
        public UserEntity Challenger { get; set; } = null!;
        public UserEntity Opponent { get; set; } = null!;
        public ICollection<ChallengeAttemptEntity> Attempts { get; set; } = new List<ChallengeAttemptEntity>();
    }
}
