namespace MDRNJfitness.Helpers.Sprites
{
    /// <summary>Draws <see cref="SpriteLayer"/>s onto a canvas with crisp, whole-pixel scaling.</summary>
    public static class SpritePainter
    {
        /// <summary>
        /// Largest whole-number scale that fits, centred horizontally and
        /// bottom-aligned (feet on the floor). Falls back to a fractional scale
        /// only when the canvas is smaller than the sprite.
        /// </summary>
        public static (float X, float Y, float Scale) Place(RectF area, int width, int height, bool bottomAlign = true)
        {
            var fit = MathF.Min(area.Width / width, area.Height / height);
            var scale = fit >= 1 ? MathF.Floor(fit) : fit;
            var y = bottomAlign
                ? area.Y + MathF.Floor(area.Height - height * scale)
                : area.Y + MathF.Floor((area.Height - height * scale) / 2);
            return (area.X + MathF.Floor((area.Width - width * scale) / 2), y, scale);
        }

        /// <summary>
        /// Draws one layer, merging same-colour runs into single rectangles.
        /// On odd frames rows above breathRow sink 1px (the row just above
        /// breathRow is dropped) so the character idles without extra art.
        /// </summary>
        public static void DrawLayer(ICanvas canvas, SpriteLayer layer, (float X, float Y, float Scale) at, int frame, int breathRow)
        {
            var breathing = frame % 2 == 1;

            for (int y = 0; y < layer.Height; y++)
            {
                var destY = y;
                if (breathing && y < breathRow)
                {
                    if (y == breathRow - 1) continue;
                    destY = y + 1;
                }
                DrawRow(canvas, layer, y, at.X, at.Y + destY * at.Scale, at.Scale, 0);
            }
        }

        /// <summary>Draws just the layer's non-empty area, centred and scaled up to fill the canvas (shop icons).</summary>
        public static void DrawCropped(ICanvas canvas, SpriteLayer layer, RectF area)
        {
            int minX = layer.Width, minY = layer.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < layer.Height; y++)
                for (int x = 0; x < layer.Width; x++)
                    if (layer.Rows[y][x] != '.')
                    {
                        minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                    }
            if (maxX < 0) return;

            // Square crop so wide and tall items keep their proportions
            var size = Math.Max(maxX - minX + 1, maxY - minY + 1);
            var at = Place(area, size, size, bottomAlign: false);
            var offsetX = at.X + (size - (maxX - minX + 1)) / 2 * at.Scale;
            var offsetY = at.Y + (size - (maxY - minY + 1)) / 2 * at.Scale;

            for (int y = minY; y <= maxY; y++)
                DrawRow(canvas, layer, y, offsetX, offsetY + (y - minY) * at.Scale, at.Scale, minX, maxX + 1);
        }

        private static void DrawRow(ICanvas canvas, SpriteLayer layer, int y, float originX, float destY, float scale, int fromX, int toX = int.MaxValue)
        {
            var row = layer.Rows[y];
            var end = Math.Min(toX, row.Length);
            for (int x = fromX; x < end;)
            {
                var ch = row[x];
                var run = x + 1;
                while (run < end && row[run] == ch) run++;

                if (ch != '.' && layer.Palette.TryGetValue(ch, out var color))
                {
                    canvas.FillColor = color;
                    canvas.FillRectangle(originX + (x - fromX) * scale, destY, (run - x) * scale, scale);
                }
                x = run;
            }
        }
    }
}
