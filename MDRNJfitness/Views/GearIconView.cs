using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers.Sprites;

namespace MDRNJfitness.Views;

/// <summary>
/// A shop / collection thumbnail: the gear item's pixels cropped and scaled
/// up crisply. Uses the default art (not a per-hero version). With
/// <see cref="HeroId"/> set to a special hero, shows that hero's outfit piece
/// for <see cref="Slot"/> instead.
/// </summary>
public class GearIconView : GraphicsView
{
    public static readonly BindableProperty ItemIdProperty =
        BindableProperty.Create(
            nameof(ItemId),
            typeof(string),
            typeof(GearIconView),
            null,
            propertyChanged: (b, _, _) => ((GearIconView)b).Invalidate());

    public string? ItemId
    {
        get => (string?)GetValue(ItemIdProperty);
        set => SetValue(ItemIdProperty, value);
    }

    public static readonly BindableProperty HeroIdProperty =
        BindableProperty.Create(
            nameof(HeroId),
            typeof(string),
            typeof(GearIconView),
            null,
            propertyChanged: (b, _, _) => ((GearIconView)b).Invalidate());

    public string? HeroId
    {
        get => (string?)GetValue(HeroIdProperty);
        set => SetValue(HeroIdProperty, value);
    }

    public static readonly BindableProperty SlotProperty =
        BindableProperty.Create(
            nameof(Slot),
            typeof(GearSlot),
            typeof(GearIconView),
            GearSlot.Head,
            propertyChanged: (b, _, _) => ((GearIconView)b).Invalidate());

    public GearSlot Slot
    {
        get => (GearSlot)GetValue(SlotProperty);
        set => SetValue(SlotProperty, value);
    }

    public GearIconView()
    {
        InputTransparent = true; // taps go to the card / slot around the icon
        Drawable = new IconDrawable(this);
    }

    private sealed class IconDrawable : IDrawable
    {
        private readonly GearIconView _view;

        public IconDrawable(GearIconView view) => _view = view;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = false;
            if (_view.HeroId is string heroId)
            {
                if (HeroSprites.TryGetHeroPiece(heroId, _view.Slot, out var piece))
                    SpritePainter.DrawCropped(canvas, piece, dirtyRect);
            }
            else if (_view.ItemId is string id && HeroSprites.TryGetGear(id, null, out var layer))
                SpritePainter.DrawCropped(canvas, layer, dirtyRect);
        }
    }
}
