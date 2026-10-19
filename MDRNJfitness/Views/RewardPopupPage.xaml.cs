using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers.Sprites;

namespace MDRNJfitness.Views;

public partial class RewardPopupPage : ContentPage
{
    /// <summary>Gains below this (e.g. a workout ended after a few seconds) aren't worth a popup.</summary>
    private const float MinGainToShow = 0.05f;

    private readonly bool _leveledUp;

    public RewardPopupPage(CharacterProgressDto? progress, AccountProgressDto? account, string headline)
    {
        InitializeComponent();

        _leveledUp = account?.LeveledUp == true;
        var completedGoals = account?.CompletedGoals ?? new List<CompletedGoalDto>();

        HeadlineLabel.Text = _leveledUp ? "LEVEL UP!"
            : progress?.Evolved == true ? "EVOLVED!"
            : completedGoals.Any(g => g.Kind == "Quest") ? "QUEST COMPLETE!"
            : completedGoals.Any(g => g.Kind == "Achievement") ? "ACHIEVEMENT!"
            : completedGoals.Count > 0 ? headline // duel results pass VICTORY! / DEFEAT
            : progress?.LevelUps.Count > 0 ? "STAT UP!"
            : headline;

        if (_leveledUp)
        {
            LevelUpBanner.IsVisible = true;
            LevelUpLabel.Text = $"LV {account!.PreviousLevel} ▶ LV {account.NewLevel}";
        }

        var outfit = progress?.Character?.OutfitSlot ?? "default";
        var avatarId = progress?.Character?.AvatarId ?? "classic";
        BeforeAvatar.AvatarId = AfterAvatar.AvatarId = HeroAvatar.AvatarId = avatarId;
        var gear = progress?.Character?.InDrawOrder() ?? Array.Empty<string>();
        BeforeAvatar.Gear = AfterAvatar.Gear = HeroAvatar.Gear = gear;

        // Full-body avatars don't wear outfits, so a before → after would look identical
        var showsOutfit = !HeroSprites.TryGetFullBodyAvatar(avatarId, out _, out _);
        if (progress?.Evolved == true && showsOutfit)
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
            if (progress?.Evolved == true)
            {
                EvolutionLabel.IsVisible = true;
                EvolutionLabel.Text = $"New build: {BodyTypeName(progress.Character?.BodyType)}";
            }
        }

        if (account?.XPGained > 0)
            AddLine($"+{account.XPGained} XP", "PxGold");
        if (account?.GoldGained > 0)
            AddLine($"+{account.GoldGained} gold", "PxOrange");
        if (_leveledUp)
        {
            AddLine($"★ +{AccountProgression.StatPointsBetween(account!.PreviousLevel, account.NewLevel)} free stat points: spend them on the Hero tab", "PxGold");
            AddLine($"★ Dungeon gold & kills now +{(AccountProgression.DungeonBoost(account.NewLevel) - 1) * 100:0}%", "PxGold");
        }
        foreach (var goal in completedGoals)
            AddLine($"✔ {goal.Kind}: {goal.Title} (+{goal.XPReward} XP)", "PxTeal");

        if (progress is not null)
        {
            AddGain("STR", progress.StrengthGained, "PxStr");
            AddGain("END", progress.CardioGained, "PxTeal");
            AddGain("FLX", progress.FlexibilityGained, "PxFlx");
            foreach (var levelUp in progress.LevelUps)
                AddLine($"★ {levelUp.Stat} reached Lv {levelUp.NewLevel}!", "PxGold");
        }
    }

    public static bool HasSomethingToShow(CharacterProgressDto? progress, AccountProgressDto? account) =>
        account?.LeveledUp == true
        || account?.CompletedGoals.Count > 0
        || progress?.Evolved == true
        || progress?.LevelUps.Count > 0
        || progress?.StrengthGained + progress?.CardioGained + progress?.FlexibilityGained >= MinGainToShow;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_leveledUp) return;

        // Pop the level banner in, then give it a little bounce
        LevelUpBanner.Scale = 0.2;
        await LevelUpBanner.ScaleToAsync(1.2, 250, Easing.CubicOut);
        await LevelUpBanner.ScaleToAsync(1.0, 150, Easing.CubicIn);
    }

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
