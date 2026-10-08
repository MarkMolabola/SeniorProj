using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.Enums;

namespace MDRNJfitness.Views;

public partial class RewardPopupPage : ContentPage
{
    /// <summary>Gains below this (e.g. a workout ended after a few seconds) aren't worth a popup.</summary>
    private const float MinGainToShow = 0.05f;

    public RewardPopupPage(CharacterProgressDto progress, string headline)
    {
        InitializeComponent();

        HeadlineLabel.Text = progress.Evolved ? "EVOLVED!"
            : progress.LevelUps.Count > 0 ? "STAT UP!"
            : headline;

        var outfit = progress.Character?.OutfitSlot ?? "default";
        if (progress.Evolved)
        {
            EvolutionRow.IsVisible = true;
            HeroBox.IsVisible = false;
            BeforeAvatar.AvatarType = progress.PreviousOutfitSlot ?? "default";
            AfterAvatar.AvatarType = outfit;

            EvolutionLabel.IsVisible = true;
            EvolutionLabel.Text = $"New look: {OutfitName(outfit)} · {BodyTypeName(progress.Character?.BodyType)}";
        }
        else
        {
            HeroAvatar.AvatarType = outfit;
        }

        AddGain("STR", progress.StrengthGained, "PxStr");
        AddGain("END", progress.CardioGained, "PxTeal");
        AddGain("FLX", progress.FlexibilityGained, "PxFlx");
        foreach (var levelUp in progress.LevelUps)
            AddLine($"★ {levelUp.Stat} reached Lv {levelUp.NewLevel}!", "PxGold");
    }

    public static bool HasSomethingToShow(CharacterProgressDto progress) =>
        progress.Evolved
        || progress.LevelUps.Count > 0
        || progress.StrengthGained + progress.CardioGained + progress.FlexibilityGained >= MinGainToShow;

    private void AddGain(string stat, float gained, string colorKey)
    {
        if (gained >= MinGainToShow)
            AddLine($"+{gained:0.#} {stat}", colorKey);
    }

    private void AddLine(string text, string colorKey)
    {
        var label = new Label { Text = text };
        if (Resource("PxBody") is Style style) label.Style = style;
        if (Resource(colorKey) is Color color) label.TextColor = color;
        RewardLines.Children.Add(label);
    }

    private static object? Resource(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true ? value : null;

    private static string OutfitName(string outfitSlot) =>
        string.IsNullOrEmpty(outfitSlot) ? "Default" : char.ToUpperInvariant(outfitSlot[0]) + outfitSlot[1..];

    private static string BodyTypeName(int? bodyType) =>
        bodyType is int i && Enum.IsDefined(typeof(BodyType), i) ? ((BodyType)i).ToString() : "Athletic";

    private async void OnContinueClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
