using MDRNJfitness.Helpers;

namespace MDRNJfitness.Views;

public partial class PixelAvatarView : ContentView
{
    // ── Bindable property so XAML can bind AvatarType to a ViewModel string ──
    public static readonly BindableProperty AvatarTypeProperty =
        BindableProperty.Create(
            nameof(AvatarType),
            typeof(string),
            typeof(PixelAvatarView),
            "default",
            propertyChanged: (b, _, _) => ((PixelAvatarView)b).OnAvatarTypeChanged());

    public string AvatarType
    {
        get => (string)GetValue(AvatarTypeProperty);
        set => SetValue(AvatarTypeProperty, value);
    }

    /// <summary>Chosen hero look (HeroAvatars id); "classic" draws the outfit layers.</summary>
    public static readonly BindableProperty AvatarIdProperty =
        BindableProperty.Create(
            nameof(AvatarId),
            typeof(string),
            typeof(PixelAvatarView),
            "classic",
            propertyChanged: (b, _, _) => ((PixelAvatarView)b).OnAvatarTypeChanged());

    public string AvatarId
    {
        get => (string)GetValue(AvatarIdProperty);
        set => SetValue(AvatarIdProperty, value);
    }

    /// <summary>Worn gear item ids in draw order (bottom first, head last).</summary>
    public static readonly BindableProperty GearProperty =
        BindableProperty.Create(
            nameof(Gear),
            typeof(IReadOnlyList<string>),
            typeof(PixelAvatarView),
            null,
            propertyChanged: (b, _, _) => ((PixelAvatarView)b).OnAvatarTypeChanged());

    public IReadOnlyList<string>? Gear
    {
        get => (IReadOnlyList<string>?)GetValue(GearProperty);
        set => SetValue(GearProperty, value);
    }

    private readonly PixelAvatarDrawable _drawable = new();
    private IDispatcherTimer? _timer;

    public PixelAvatarView()
    {
        InitializeComponent();
        AvatarCanvas.Drawable = _drawable;
    }

    // ── Lifecycle: start/stop the animation timer when the view attaches ──────
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is not null)
            StartAnimation();
        else
            StopAnimation();
    }

    private void StartAnimation()
    {
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(400);
        _timer.Tick += (_, _) =>
        {
            _drawable.Frame = (_drawable.Frame + 1) % 2;
            AvatarCanvas.Invalidate();
        };
        _timer.Start();
    }

    private void StopAnimation()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void OnAvatarTypeChanged()
    {
        _drawable.AvatarType = AvatarType ?? "default";
        _drawable.AvatarId = AvatarId ?? "classic";
        _drawable.Gear = Gear ?? Array.Empty<string>();
        _drawable.Frame = 0;
        AvatarCanvas.Invalidate();
    }
}
