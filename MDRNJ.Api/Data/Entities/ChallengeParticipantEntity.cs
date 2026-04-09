using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    public class ChallengeParticipantEntity
    {
        public Guid ChallengeId { get; set; }
        public Guid UserId { get; set; }
        public float Progress { get; set; }
        public ChallengeStatus Status { get; set; } = ChallengeStatus.Pending;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ChallengeEntity Challenge { get; set; } = null!;
        public UserEntity User { get; set; } = null!;
    }
}
