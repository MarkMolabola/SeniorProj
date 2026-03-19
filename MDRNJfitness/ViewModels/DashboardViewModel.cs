using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Helpers;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IGameService _game;
        private readonly ICharacterService _character;
        private readonly IWorkoutService _workout;

        [ObservableProperty] private GameStatsDto? _stats;
        [ObservableProperty] private CharacterStateDto? _character2;
        public ObservableCollection<WorkoutDto> RecentWorkouts { get; } = new();

        public DashboardViewModel(IGameService game, ICharacterService character, IWorkoutService workout)
        {
            _game = game;
            _character = character;
            _workout = workout;
            Title = "Dashboard";

            WeakReferenceMessenger.Default.Register<CharacterEvolvedMessage>(this, (_, m) =>
                Character2 = m.Character);
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                Stats = await _game.GetStatsAsync();
                Character2 = await _character.GetCharacterAsync();

                var history = await _workout.GetHistoryAsync(1, 5);
                RecentWorkouts.Clear();
                foreach (var w in history) RecentWorkouts.Add(w);
            });
    }
}
