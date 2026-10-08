using CommunityToolkit.Mvvm.ComponentModel;

namespace MDRNJfitness.Models.Character
{
    /// <summary>One look in the Hero tab's customize panel.</summary>
    public partial class HeroAvatarOption : ObservableObject
    {
        public HeroAvatarOption(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }

        [ObservableProperty] private bool _isSelected;

        /// <summary>Outfit to preview the layered hero in (full-body avatars ignore it).</summary>
        [ObservableProperty] private string _previewOutfit = "default";

        /// <summary>Currently worn gear, so previews show the hero as it would look.</summary>
        [ObservableProperty] private IReadOnlyList<string> _previewGear = Array.Empty<string>();
    }
}
