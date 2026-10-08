using CommunityToolkit.Mvvm.Messaging;
using MDRNJ.Shared.DTOs.Character;
using MDRNJfitness.Helpers;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.Views;

namespace MDRNJfitness.Services
{
    public class CharacterProgressPresenter : ICharacterProgressPresenter
    {
        public async Task PresentAsync(CharacterProgressDto? progress, string headline)
        {
            if (progress is null || !RewardPopupPage.HasSomethingToShow(progress))
                return;

            if (progress.Evolved && progress.Character is not null)
                WeakReferenceMessenger.Default.Send(new CharacterEvolvedMessage(progress.Character));

            await Shell.Current.Navigation.PushModalAsync(new RewardPopupPage(progress, headline));
        }
    }
}
