using MDRNJfitness.Helpers;

namespace MDRNJfitness.Views;

/// <summary>The animated dungeon on the Home tab (see <see cref="DungeonDrawable"/>).</summary>
public class DungeonView : GraphicsView
{
    public static readonly BindableProperty AvatarTypeProperty =
        BindableProperty.Create(nameof(AvatarType), typeof(string), typeof(DungeonView), "default",
            propertyChanged: (b, _, _) => ((DungeonView)b).Sync());

    public static readonly BindableProperty AvatarIdProperty =
        BindableProperty.Create(nameof(AvatarId), typeof(string), typeof(DungeonView), "classic",
            propertyChanged: (b, _, _) => ((DungeonView)b).Sync());

    public static readonly BindableProperty GearProperty =
        BindableProperty.Create(nameof(Gear), typeof(IReadOnlyList<string>), typeof(DungeonView), null,
            propertyChanged: (b, _, _) => ((DungeonView)b).Sync());

    public static readonly BindableProperty MonsterIdProperty =
        BindableProperty.Create(nameof(MonsterId), typeof(string), typeof(DungeonView), "slime",
            propertyChanged: (b, _, _) => ((DungeonView)b).OnMonsterChanged());

    public string AvatarType { get => (string)GetValue(AvatarTypeProperty); set => SetValue(AvatarTypeProperty, value); }
    public string AvatarId { get => (string)GetValue(AvatarIdProperty); set => SetValue(AvatarIdProperty, value); }
    public IReadOnlyList<string>? Gear { get => (IReadOnlyList<string>?)GetValue(GearProperty); set => SetValue(GearProperty, value); }

    /// <summary>Toughest monster for the hero's power (DungeonDto.MonsterId).</summary>
    public string MonsterId { get => (string)GetValue(MonsterIdProperty); set => SetValue(MonsterIdProperty, value); }

    private readonly DungeonDrawable _drawable = new();
    private IDispatcherTimer? _timer;

    public DungeonView()
    {
        Drawable = _drawable;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is null)
        {
            _timer?.Stop();
            _timer = null;
            return;
        }

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(120);
        _timer.Tick += (_, _) =>
        {
            _drawable.Advance();
            Invalidate();
        };
        _timer.Start();
    }

    private void Sync()
    {
        _drawable.AvatarType = AvatarType ?? "default";
        _drawable.AvatarId = AvatarId ?? "classic";
        _drawable.Gear = Gear ?? Array.Empty<string>();
        Invalidate();
    }

    private void OnMonsterChanged()
    {
        _drawable.MonsterId = MonsterId ?? "slime";
        _drawable.ResetMonster();
        Invalidate();
    }
}
