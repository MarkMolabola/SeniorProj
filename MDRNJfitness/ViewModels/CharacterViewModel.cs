using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.Game;
using MDRNJfitness.Models.Character;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class CharacterViewModel : BaseViewModel
    {
        private readonly ICharacterService _character;

        [ObservableProperty] private CharacterStateDto? _characterState;
        [ObservableProperty] private ActivityScoreDto? _activityScore;
        [ObservableProperty] private string _avatarId = HeroAvatars.Classic;
        [ObservableProperty] private bool _isCustomizing;

        private string _spriteState = "default";
        public string SpriteState
        {
            get => _spriteState;
            set { _spriteState = value; OnPropertyChanged(); }
        }

        public ObservableCollection<HeroAvatarOption> AvatarOptions { get; } =
            new(HeroAvatars.All.Select(a => new HeroAvatarOption(a.Id, a.Name)));

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
                Show(CharacterState);
            });

        private void Show(CharacterStateDto? state)
        {
            SpriteState = state?.OutfitSlot ?? "default";
            AvatarId = state?.AvatarId ?? HeroAvatars.Classic;
            foreach (var option in AvatarOptions)
            {
                option.IsSelected = option.Id == AvatarId;
                option.PreviewOutfit = SpriteState;
            }
        }

        [RelayCommand]
        public void ToggleCustomize() => IsCustomizing = !IsCustomizing;

        [RelayCommand]
        public async Task SelectAvatarAsync(HeroAvatarOption option)
        {
            if (option.Id == AvatarId) return;

            await RunSafeAsync(async () =>
            {
                CharacterState = await _character.SetAvatarAsync(option.Id);
                Show(CharacterState);
            });
        }
    }
}
