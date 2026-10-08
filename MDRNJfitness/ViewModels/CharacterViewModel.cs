using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Character;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.ViewModels
{
    public partial class CharacterViewModel : BaseViewModel
    {
        private readonly ICharacterService _character;

        [ObservableProperty] private CharacterStateDto? _characterState;
        [ObservableProperty] private ActivityScoreDto? _activityScore;

        private string _spriteState = "default";
        public string SpriteState
        {
            get => _spriteState;
            set { _spriteState = value; OnPropertyChanged(); }
        }

        public CharacterViewModel(ICharacterService character)
        {
            _character = character;
            Title = "Character";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                CharacterState = await _character.GetCharacterAsync();
                ActivityScore = await _character.GetActivityScoreAsync();
                SpriteState = CharacterState?.OutfitSlot ?? "default";
            });
    }
}
