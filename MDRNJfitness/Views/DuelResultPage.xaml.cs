namespace MDRNJfitness.Views;

public partial class DuelResultPage : ContentPage
{
    private const double HeroSize = 96;

    /// <summary>One side of the duel as drawn by PixelAvatarView.</summary>
    public record Fighter(string Name, string Outfit, string AvatarId, IReadOnlyList<string> Gear)
    {
        public static Fighter Plain(string name) => new(name, "default", "classic", Array.Empty<string>());
    }

    private readonly string _outcome;
    private readonly TaskCompletionSource _closed = new();
    private bool _played;

    private DuelResultPage(Fighter me, Fighter them, string outcome, string title, string myScore, string theirScore)
    {
        InitializeComponent();
        _outcome = outcome;

        Dress(MyHero, me);
        Dress(TheirHero, them);
        MyNameLabel.Text = me.Name;
        TheirNameLabel.Text = them.Name;
        TitleLabel.Text = $"{title} DUEL";
        ScoreLabel.Text = $"{myScore}  vs  {theirScore}";

        (ResultLabel.Text, var colorKey) = outcome switch
        {
            "Won" => ("VICTORY!", "PxGold"),
            "Tie" => ("DRAW!", "PxTeal"),
            _ => ("DEFEAT", "PxStr")
        };
        if (Application.Current?.Resources.TryGetValue(colorKey, out var color) == true && color is Color c)
            ResultLabel.TextColor = c;
    }

    /// <summary>Plays the fight and waits until the player closes it.</summary>
    public static async Task ShowAsync(Fighter me, Fighter them, string outcome, string title, string myScore, string theirScore)
    {
        var page = new DuelResultPage(me, them, outcome, title, myScore, theirScore);
        await Shell.Current.Navigation.PushModalAsync(page);
        await page._closed.Task;
    }

    private static void Dress(PixelAvatarView hero, Fighter fighter)
    {
        hero.AvatarType = fighter.Outfit;
        hero.AvatarId = fighter.AvatarId;
        hero.Gear = fighter.Gear;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_played) return;
        _played = true;

        try
        {
            await PlayAsync();
        }
        finally
        {
            ContinueButton.IsEnabled = true;
            await ResultPanel.FadeToAsync(1, 250);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _closed.TrySetResult(); // also covers the Android back button
    }

    // ── The fight ───────────────────────────────────────────────────────

    private async Task PlayAsync()
    {
        // Wait for layout so the arena width is known
        for (var i = 0; i < 20 && Arena.Width <= 0; i++)
            await Task.Delay(25);
        var gap = Math.Max(0, Arena.Width - 2 * HeroSize);
        var offstage = Arena.Width / 2 + HeroSize;

        // Walk on, VS
        MyHero.TranslationX = -offstage;
        TheirHero.TranslationX = offstage;
        await Task.WhenAll(
            MyHero.TranslateToAsync(0, 0, 400, Easing.CubicOut),
            TheirHero.TranslateToAsync(0, 0, 400, Easing.CubicOut));
        VsLabel.Scale = 0.3;
        await Task.WhenAll(VsLabel.FadeToAsync(1, 150), VsLabel.ScaleToAsync(1.3, 250, Easing.CubicOut));
        await Task.Delay(350);
        await VsLabel.FadeToAsync(0, 150);

        switch (_outcome)
        {
            case "Tie":
                await ClashAsync(gap);
                break;
            case "Won":
                await KnockOutAsync(MyHero, TheirHero, dir: 1, gap);
                break;
            default:
                await KnockOutAsync(TheirHero, MyHero, dir: -1, gap);
                break;
        }
    }

    /// <summary>
    /// Three quick hits; the last one knocks the loser flat. <paramref name="dir"/> is +1 when
    /// the winner attacks left to right.
    /// </summary>
    private async Task KnockOutAsync(VisualElement winner, VisualElement loser, int dir, double gap)
    {
        var reach = dir * (gap + HeroSize * 0.3);
        string[] hits = { "POW!", "BAM!", "WHAM!" };

        for (var i = 0; i < hits.Length; i++)
        {
            var final = i == hits.Length - 1;
            await winner.TranslateToAsync(reach, 0, final ? 170u : 110u, Easing.CubicIn);
            _ = PopImpactAsync(hits[i], dir * (Arena.Width / 2 - HeroSize));

            if (final)
            {
                // Sent flying: knocked back, tipped over, dazed
                await Task.WhenAll(
                    winner.TranslateToAsync(reach * 0.7, 0, 120),
                    loser.TranslateToAsync(dir * 34, 22, 320, Easing.CubicOut),
                    loser.RotateToAsync(dir * 90, 320, Easing.CubicOut),
                    loser.FadeToAsync(0.55, 320));
            }
            else
            {
                await Task.WhenAll(
                    winner.TranslateToAsync(reach * 0.7, 0, 90),
                    FlinchAsync(loser, dir));
            }
        }

        await winner.TranslateToAsync(0, 0, 260, Easing.CubicOut);
        await PopKoAsync("K.O.!");

        // Victory hops
        for (var i = 0; i < 2; i++)
        {
            await winner.TranslateToAsync(0, -18, 140, Easing.CubicOut);
            await winner.TranslateToAsync(0, 0, 140, Easing.CubicIn);
        }
    }

    /// <summary>Draw: both charge, meet in the middle and bounce back.</summary>
    private async Task ClashAsync(double gap)
    {
        var meet = gap / 2 + HeroSize * 0.15;
        for (var i = 0; i < 2; i++)
        {
            await Task.WhenAll(
                MyHero.TranslateToAsync(meet, 0, 150, Easing.CubicIn),
                TheirHero.TranslateToAsync(-meet, 0, 150, Easing.CubicIn));
            _ = PopImpactAsync("CLASH!", 0);
            await Task.WhenAll(
                MyHero.TranslateToAsync(-12, 0, 220, Easing.CubicOut),
                TheirHero.TranslateToAsync(12, 0, 220, Easing.CubicOut));
            await Task.WhenAll(
                MyHero.TranslateToAsync(0, 0, 120),
                TheirHero.TranslateToAsync(0, 0, 120));
        }
        await PopKoAsync("DRAW");
    }

    private static async Task FlinchAsync(VisualElement hero, int dir)
    {
        await Task.WhenAll(hero.TranslateToAsync(dir * 10, 0, 60), hero.FadeToAsync(0.4, 60));
        await Task.WhenAll(hero.TranslateToAsync(0, 0, 90), hero.FadeToAsync(1, 90));
    }

    private async Task PopImpactAsync(string text, double x)
    {
        ImpactLabel.Text = text;
        ImpactLabel.TranslationX = x;
        ImpactLabel.Opacity = 1;
        ImpactLabel.Scale = 0.4;
        await ImpactLabel.ScaleToAsync(1.3, 140, Easing.CubicOut);
        await ImpactLabel.FadeToAsync(0, 160);
    }

    private async Task PopKoAsync(string text)
    {
        KoLabel.Text = text;
        KoLabel.Scale = 0.2;
        await Task.WhenAll(KoLabel.FadeToAsync(1, 120), KoLabel.ScaleToAsync(1.25, 240, Easing.CubicOut));
        await KoLabel.ScaleToAsync(1.0, 140, Easing.CubicIn);
    }

    private async void OnContinueClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
