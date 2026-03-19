using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Challenge
{
    public class ChallengeParticipantDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public float Progress { get; set; }
        public bool IsCompleted { get; set; }
    }
}
