namespace MDRNJfitness.Controls;

public partial class XPBarControl : ContentView
{
    // ── Bindable properties ───────────────────────────────────────────────
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(XPBarControl), 0d,
            propertyChanged: OnBarChanged);

    public static readonly BindableProperty FillColorProperty =
        BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(XPBarControl), Color.FromArgb("#FFC94A"),
            propertyChanged: OnBarChanged);

    public static readonly BindableProperty TrackColorProperty =
        BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(XPBarControl), Color.FromArgb("#3A3270"),
            propertyChanged: OnBarChanged);

    public static readonly BindableProperty SegmentedProperty =
        BindableProperty.Create(nameof(Segmented), typeof(bool), typeof(XPBarControl), true,
            propertyChanged: OnBarChanged);

    /// <summary>0–1. Values outside the range are clamped.</summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    /// <summary>When true the bar is drawn as blocks like a retro health bar.</summary>
    public bool Segmented
    {
        get => (bool)GetValue(SegmentedProperty);
        set => SetValue(SegmentedProperty, value);
    }

    private readonly PixelBarDrawable _drawable = new();

    public XPBarControl()
    {
        InitializeComponent();
        BarCanvas.Drawable = _drawable;
        Sync();
    }

    private static void OnBarChanged(BindableObject bindable, object oldValue, object newValue)
        => ((XPBarControl)bindable).Sync();

    private void Sync()
    {
        _drawable.Progress = Math.Clamp(Progress, 0, 1);
        _drawable.FillColor = FillColor;
        _drawable.TrackColor = TrackColor;
        _drawable.Segmented = Segmented;
        BarCanvas?.Invalidate();
    }

    private sealed class PixelBarDrawable : IDrawable
    {
        private const float Border = 2f;
        private const float SegmentWidth = 10f;
        private const float Gap = 2f;
        private static readonly Color Ink = Color.FromArgb("#0B0918");

        public double Progress { get; set; }
        public Color FillColor { get; set; } = Colors.Gold;
        public Color TrackColor { get; set; } = Colors.Gray;
        public bool Segmented { get; set; } = true;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = false;

            // Ink outline + background (also shows through the gaps between blocks)
            canvas.FillColor = Ink;
            canvas.FillRectangle(dirtyRect);

            var inner = new RectF(
                dirtyRect.X + Border, dirtyRect.Y + Border,
                Math.Max(0, dirtyRect.Width - Border * 2), Math.Max(0, dirtyRect.Height - Border * 2));

            DrawBlocks(canvas, inner, TrackColor);

            var fillWidth = (float)Math.Round(inner.Width * Progress);
            if (fillWidth <= 0) return;

            canvas.SaveState();
            canvas.ClipRectangle(inner.X, inner.Y, fillWidth, inner.Height);
            DrawBlocks(canvas, inner, FillColor);
            canvas.RestoreState();
        }

        private void DrawBlocks(ICanvas canvas, RectF area, Color color)
        {
            canvas.FillColor = color;
            if (!Segmented)
            {
                canvas.FillRectangle(area);
                return;
            }

            for (var x = area.X; x < area.Right; x += SegmentWidth + Gap)
                canvas.FillRectangle(x, area.Y, Math.Min(SegmentWidth, area.Right - x), area.Height);
        }
    }
}
