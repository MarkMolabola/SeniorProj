using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>One submitted result in a challenge (reps, seconds or km).</summary>
    public class ChallengeAttemptEntity
    {
        public Guid AttemptId { get; set; } = Guid.NewGuid();
        public Guid ChallengeId { get; set; }
        public Guid UserId { get; set; }
        public float Value { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Storage path of the proof video / photo (see IProofStorage), if one was required.</summary>
        public string? ProofPath { get; set; }

        // Navigation
        public ChallengeEntity Challenge { get; set; } = null!;
    }
}
