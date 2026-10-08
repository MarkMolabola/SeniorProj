namespace MDRNJfitness.Helpers.Sprites
{
    /// <summary>
    /// One transparent layer of a pixel-art sprite (body, outfit, equipment…).
    /// Each row is a string of palette keys; '.' is transparent. All layers of a
    /// character share the same canvas size so they stack pixel-for-pixel.
    /// </summary>
    public sealed class SpriteLayer
    {
        public SpriteLayer(string[] rows, IReadOnlyDictionary<char, Color> palette)
        {
            if (rows.Length == 0)
                throw new ArgumentException("A sprite layer needs at least one row.", nameof(rows));

            Width = rows[0].Length;
            if (rows.Any(r => r.Length != Width))
                throw new ArgumentException("All rows of a sprite layer must be the same width.", nameof(rows));

            Rows = rows;
            Palette = palette;
        }

        public string[] Rows { get; }
        public IReadOnlyDictionary<char, Color> Palette { get; }
        public int Width { get; }
        public int Height => Rows.Length;
    }
}
