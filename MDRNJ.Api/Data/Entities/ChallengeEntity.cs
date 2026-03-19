using MDRNJ.Shared.Enums;
using System;
using System.Collections.Generic;

namespace MDRNJ.Api.Data.Entities
{
    public class ChallengeEntity
    {
        public Guid ChallengeId { get; set; } = Guid.NewGuid();
        public Guid CreatorId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public ChallengeType ChallengeType { get; set; }
        public float TargetValue { get; set; }
        public int XPReward { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ChallengeStatus Status { get; set; } = ChallengeStatus.Pending;

        // Navigation
        public UserEntity Creator { get; set; } = null!;
        public ICollection<ChallengeParticipantEntity> Participants { get; set; } = new List<ChallengeParticipantEntity>();
    }
}
