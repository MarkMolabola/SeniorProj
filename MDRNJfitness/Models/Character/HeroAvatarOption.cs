using CommunityToolkit.Mvvm.ComponentModel;
using MDRNJ.Shared.Game;

namespace MDRNJfitness.Models.Character
{
    /// <summary>One look in the Hero tab's customize panel.</summary>
    public partial class HeroAvatarOption : ObservableObject
    {
        public HeroAvatarOption(HeroAvatar hero) => Hero = hero;

        public HeroAvatar Hero { get; }
        public string Id => Hero.Id;
        public string Name => Hero.Name;

        public Brush RarityBrush => Hero.IsSpecial ? RarityColors.BrushFor(Hero.Rarity) : new SolidColorBrush(Colors.Transparent);
        public string PriceText => Hero.Price.ToString();

        [ObservableProperty] private bool _isSelected;

        /// <summary>A special hero the player hasn't bought yet.</summary>
        [ObservableProperty] private bool _isLocked;

        /// <summary>Outfit to preview the layered hero in (full-body avatars ignore it).</summary>
        [ObservableProperty] private string _previewOutfit = "default";

        /// <summary>Currently worn gear, so previews show the hero as it would look.</summary>
        [ObservableProperty] private IReadOnlyList<string> _previewGear = Array.Empty<string>();
    }
}
