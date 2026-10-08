using MDRNJ.Shared.DTOs.Character;

namespace MDRNJfitness.Services.Interfaces
{
    public interface ICharacterService
    {
        Task<CharacterStateDto> GetCharacterAsync();
        Task<CharacterStateDto> SetAvatarAsync(string avatarId);
        Task<ActivityScoreDto> GetActivityScoreAsync();
        Task<ActivityScoreDto> RecalculateActivityScoreAsync();
    }
}
