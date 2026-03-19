using MDRNJ.Shared.DTOs.Character;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface ICharacterService
    {
        Task<CharacterStateDto> GetCharacterAsync(Guid userId);
        Task<CharacterStateDto> EvolveCharacterAsync(Guid userId);
        Task<ActivityScoreDto> GetActivityScoreAsync(Guid userId);
        Task<ActivityScoreDto> RecalculateActivityScoreAsync(Guid userId);
    }
}
