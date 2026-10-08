using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Challenge
{
    /// <summary>Challenge a friend to a 1v1 contest.</summary>
    public class CreateChallengeDto
    {
        [Required]
        public required Guid OpponentId { get; set; }

        /// <summary>A <see cref="MDRNJ.Shared.Enums.DuelContest"/> name, e.g. "PushUps".</summary>
        [Required]
        [MaxLength(20)]
        public required string Contest { get; set; }

        /// <summary>A <see cref="MDRNJ.Shared.Enums.DuelScoring"/> name, e.g. "BestAttempt".</summary>
        [Required]
        [MaxLength(20)]
        public required string Scoring { get; set; }

        /// <summary>Race distance; required for a best-attempt run.</summary>
        [Range(0.1, 100)]
        public float? DistanceKm { get; set; }

        /// <summary>One of <see cref="MDRNJ.Shared.Game.DuelRules.DurationHours"/>.</summary>
        public int DurationHours { get; set; } = 72;
    }

    public class SubmitAttemptDto
    {
        /// <summary>Reps, seconds, or km, per <see cref="MDRNJ.Shared.Game.DuelRules.Unit"/>.</summary>
        [Range(0.01, 100000)]
        public float Value { get; set; }
    }
}
