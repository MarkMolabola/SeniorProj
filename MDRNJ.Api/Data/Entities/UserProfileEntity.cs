using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    public class UserProfileEntity
    {
        public Guid ProfileId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? Bio { get; set; }
        public float? WeightKg { get; set; }
        public float? HeightCm { get; set; }
        public FitnessGoal FitnessGoal { get; set; } = FitnessGoal.GeneralFitness;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
