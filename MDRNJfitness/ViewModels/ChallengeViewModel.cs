using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Challenge;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class ChallengeViewModel : BaseViewModel
    {
        private readonly IChallengeService _challenges;

        public ObservableCollection<ChallengeDto> ActiveChallenges { get; } = new();
        public ObservableCollection<ChallengeDto> MyChallenges { get; } = new();

        public ChallengeViewModel(IChallengeService challenges)
        {
            _challenges = challenges;
            Title = "Challenges";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var active = await _challenges.GetActiveChallengesAsync();
                ActiveChallenges.Clear();
                foreach (var c in active) ActiveChallenges.Add(c);

                var mine = await _challenges.GetMyChallengesAsync();
                MyChallenges.Clear();
                foreach (var c in mine) MyChallenges.Add(c);
            });

        [RelayCommand]
        public async Task JoinAsync(ChallengeDto challenge) =>
            await RunSafeAsync(async () =>
            {
                var joined = await _challenges.JoinAsync(challenge.ChallengeId);
                MyChallenges.Add(joined);
                await Shell.Current.DisplayAlertAsync("Joined!", $"You joined: {challenge.Title}", "OK");
            });
    }
}
