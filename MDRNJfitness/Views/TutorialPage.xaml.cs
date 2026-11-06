using MDRNJ.Shared.Game;

namespace MDRNJfitness.Views;

public partial class TutorialPage : ContentPage
{
    /// <param name="HeroId">Hero to show, or null for the dungeon scene.</param>
    /// <param name="Place">Where this lives in the app; null hides the chip.</param>
    private sealed record Slide(string Title, string Body, string? HeroId, string? Place = null, string? Icon = null);

    private static readonly Slide[] Slides =
    {
        new("WELCOME, HERO!",
            "Your real workouts power a pixel hero. Train in real life and your hero grows stronger.",
            HeroAvatars.Classic),
        new("TRAIN",
            "Choose a workout, press START, and tap the timer when you're done. You earn XP, gold and stat points when you finish.",
            "starting_ninja", "TRAIN TAB", "tab_train.png"),
        new("LEVEL UP",
            "Workouts raise your STR, END and FLX. Each level gives you free stat points to spend on the Hero tab.",
            "starting_buff_ninja", "HERO TAB", "tab_hero.png"),
        new("THE DUNGEON",
            $"Your hero fights monsters even while you're away. Gold builds up for {DungeonRules.CapHours} hours, so come back and tap COLLECT.",
            null, "HOME TAB", "tab_home.png"),
        new("GEAR UP",
            "Spend your gold in the Shop on gear and new heroes. Better gear makes your hero stronger in the dungeon.",
            "tennis_avatar", "HOME ▶ SHOP", "tab_home.png"),
        new("QUESTS AND DUELS",
            "Complete quests for bonus rewards. In the Guild, add friends, challenge them to duels, climb the ranks and chat.\n\nSee this again anytime: Hero ▶ Profile ▶ HOW TO PLAY.",
            "starter_archer", "QUESTS + GUILD TABS", "tab_guild.png"),
    };

    private int _index;
    private bool _closing;

    public TutorialPage()
    {
        InitializeComponent();

        foreach (var _ in Slides)
            Dots.Children.Add(new BoxView { WidthRequest = 10, HeightRequest = 10 });

        ShowSlide();
    }

    private void ShowSlide()
    {
        var slide = Slides[_index];
        var isLast = _index == Slides.Length - 1;

        TitleLabel.Text = slide.Title;
        BodyLabel.Text = slide.Body;

        Hero.IsVisible = slide.HeroId is not null;
        Dungeon.IsVisible = slide.HeroId is null;
        if (slide.HeroId is not null)
            Hero.AvatarId = slide.HeroId;

        PlaceChip.IsVisible = slide.Place is not null;
        PlaceLabel.Text = slide.Place;
        PlaceIcon.Source = slide.Icon;

        // No BACK on the first slide, so NEXT takes the whole row
        BackButton.IsVisible = _index > 0;
        Grid.SetColumn(NextButton, _index > 0 ? 1 : 0);
        Grid.SetColumnSpan(NextButton, _index > 0 ? 1 : 2);
        SkipButton.IsVisible = !isLast;
        NextButton.Text = isLast ? "LET'S GO!" : "NEXT";

        for (var i = 0; i < Dots.Children.Count; i++)
            ((BoxView)Dots.Children[i]).Color = Resource(i == _index ? "PxGold" : "PxTrack");
    }

    private void Move(int step)
    {
        var next = _index + step;
        if (next < 0 || next >= Slides.Length) return;
        _index = next;
        ShowSlide();
    }

    private async Task CloseAsync()
    {
        if (_closing) return; // a double tap would pop the page underneath too
        _closing = true;
        await Navigation.PopModalAsync();
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (_index == Slides.Length - 1)
            await CloseAsync();
        else
            Move(1);
    }

    private void OnBackClicked(object? sender, EventArgs e) => Move(-1);

    private async void OnDoneClicked(object? sender, EventArgs e) => await CloseAsync();

    private void OnSwipedLeft(object? sender, SwipedEventArgs e) => Move(1);

    private void OnSwipedRight(object? sender, SwipedEventArgs e) => Move(-1);

    private static Color Resource(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color : Colors.Gray;
}
