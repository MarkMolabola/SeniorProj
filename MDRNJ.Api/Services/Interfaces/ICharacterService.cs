using MDRNJ.Shared.DTOs.Character;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface ICharacterService
    {
        Task<CharacterStateDto> GetCharacterAsync(Guid userId);

        /// <summary>Changes the hero look (cosmetic only).</summary>
        Task<CharacterStateDto> SetAvatarAsync(Guid userId, string avatarId);

        /// <summary>
        /// Adds stat points, refreshes training shares and evolves the character
        /// (outfit / body type) when their training has shifted. Called after
        /// workouts and quest / challenge completions.
        /// </summary>
        Task<CharacterProgressDto> ApplyActivityAsync(Guid userId, StatGain gain);

        Task<ActivityScoreDto> GetActivityScoreAsync(Guid userId);
        Task<ActivityScoreDto> RecalculateActivityScoreAsync(Guid userId);

        /// <summary>Moves free stat points from levelling up into STR, END or FLX.</summary>
        Task<ActivityScoreDto> SpendStatPointsAsync(Guid userId, string stat, int points);
    }
}
