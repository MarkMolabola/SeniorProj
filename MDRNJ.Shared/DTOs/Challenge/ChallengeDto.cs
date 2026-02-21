using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Challenge
{
    public class ChallengeDto
    {
        public Guid ChallengeId { get; set; }
        public Guid CreatorId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string ChallengeType { get; set; }
        public float TargetValue { get; set; }
        public int XPReward { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public float Progress { get; set; }
        public List<ChallengeParticipantDto> Participants { get; set; } = new();
    }
}
