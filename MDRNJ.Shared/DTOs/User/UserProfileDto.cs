using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.User
{
    public class UserProfileDto
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; }
        public string? Bio { get; set; }
        public float? WeightKg { get; set; }
        public float? HeightCm { get; set; }
        public string FitnessGoal { get; set; }
    }
}
