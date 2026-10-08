using MDRNJfitness.Helpers.Sprites;

namespace MDRNJfitness.Views;

/// <summary>
/// A shop / collection thumbnail: the gear item's pixels cropped and scaled
/// up crisply. Uses the default art (not a per-hero version).
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

    public GearIconView()
    {
        Drawable = new IconDrawable(this);
    }

    private sealed class IconDrawable : IDrawable
    {
        private readonly GearIconView _view;

        public IconDrawable(GearIconView view) => _view = view;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = false;
            if (_view.ItemId is string id && HeroSprites.TryGetGear(id, null, out var layer))
                SpritePainter.DrawCropped(canvas, layer, dirtyRect);
        }
    }
}
