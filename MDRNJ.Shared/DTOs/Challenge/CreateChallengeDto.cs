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

        /// <summary>Both players must attach a video / photo to every result.</summary>
        public bool RequireProof { get; set; }

        /// <summary>With proof on: results go to a review window where either player can dispute.</summary>
        public bool AllowDisputes { get; set; }
    }

    public class SubmitAttemptDto
    {
        /// <summary>Reps, seconds, or km, per <see cref="MDRNJ.Shared.Game.DuelRules.Unit"/>.</summary>
        [Range(0.01, 100000)]
        public float Value { get; set; }

        /// <summary>The uploaded proof (<see cref="ProofUploadDto.ProofPath"/>); required when the challenge requires proof.</summary>
        [MaxLength(300)]
        public string? ProofPath { get; set; }
    }

    public class ProofUploadRequestDto
    {
        /// <summary>The file's MIME type, e.g. "video/mp4" or "image/jpeg".</summary>
        [Required]
        [MaxLength(100)]
        public required string ContentType { get; set; }
    }

    /// <summary>Where to PUT the proof file, then submit the attempt with <see cref="ProofPath"/>.</summary>
    public class ProofUploadDto
    {
        /// <summary>Absolute (cloud storage) or relative to the API base URL (local storage).</summary>
        public string UploadUrl { get; set; } = string.Empty;
        public string ProofPath { get; set; } = string.Empty;

        /// <summary>Extra headers for the PUT (e.g. x-ms-blob-type).</summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        public long MaxBytes { get; set; }
    }

    public class DisputeChallengeDto
    {
        [MaxLength(200)]
        public string? Reason { get; set; }
    }

    /// <summary>A short-lived link to view one proof.</summary>
    public class ProofLinkDto
    {
        /// <summary>Absolute, or relative to the API base URL.</summary>
        public string Url { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
    }
}
