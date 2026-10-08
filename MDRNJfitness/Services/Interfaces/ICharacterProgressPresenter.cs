using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;

namespace MDRNJfitness.Services.Interfaces
{
    public interface ICharacterProgressPresenter
    {
        /// <summary>
        /// Shows the reward / level-up / evolution popup for the progress returned
        /// by the API. Does nothing when there's nothing worth showing.
        /// </summary>
        /// <param name="headline">Title when nothing levelled up or evolved, e.g. "WORKOUT COMPLETE".</param>
        Task PresentAsync(CharacterProgressDto? progress, AccountProgressDto? account, string headline);
    }
}
