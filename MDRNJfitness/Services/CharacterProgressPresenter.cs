using CommunityToolkit.Mvvm.Messaging;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;
using MDRNJfitness.Helpers;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.Views;

namespace MDRNJfitness.Services
{
    public class CharacterProgressPresenter : ICharacterProgressPresenter
    {
        public async Task PresentAsync(CharacterProgressDto? progress, AccountProgressDto? account, string headline)
        {
            if (!RewardPopupPage.HasSomethingToShow(progress, account))
                return;

            if (progress is { Evolved: true, Character: not null })
                WeakReferenceMessenger.Default.Send(new CharacterEvolvedMessage(progress.Character));

            await Shell.Current.Navigation.PushModalAsync(new RewardPopupPage(progress, account, headline));
        }
    }
}
