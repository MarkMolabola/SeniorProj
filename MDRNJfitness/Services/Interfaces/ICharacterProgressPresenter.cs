using MDRNJ.Shared.DTOs.Character;

namespace MDRNJfitness.Services.Interfaces
{
    public interface ICharacterProgressPresenter
    {
        /// <summary>
        /// Shows the reward / evolution popup for a CharacterProgress returned by
        /// the API. Does nothing when there's nothing worth showing.
        /// </summary>
        /// <param name="headline">Title when nothing levelled up or evolved, e.g. "WORKOUT COMPLETE".</param>
        Task PresentAsync(CharacterProgressDto? progress, string headline);
    }
}
