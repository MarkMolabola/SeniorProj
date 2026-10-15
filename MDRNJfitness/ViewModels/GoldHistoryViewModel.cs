using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJfitness.Models.Gamification;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>Where the player's gold came from and went (tap the gold chip on Home).</summary>
    public partial class GoldHistoryViewModel : BaseViewModel
    {
        private readonly IGameService _game;

        public ObservableCollection<GoldEntryCard> Entries { get; } = new();
        [ObservableProperty] private string _goldText = "0";
        [ObservableProperty] private bool _hasNoEntries;

        public GoldHistoryViewModel(IGameService game)
        {
            _game = game;
            Title = "Gold";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var stats = await _game.GetStatsAsync();
                GoldText = stats.UnlimitedGold ? "∞" : stats.Gold.ToString();

                var history = await _game.GetGoldHistoryAsync();
                Entries.Clear();
                foreach (var g in history) Entries.Add(new GoldEntryCard(g));
                HasNoEntries = Entries.Count == 0;
            });
    }
}
