using Microsoft.Maui.Graphics;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// Renders a 10×16 pixel-art character sprite scaled to fill the canvas.
    /// Set AvatarType to "default", "cardio", "strength", or "flexibility".
    /// Set Frame to 0 or 1 to select the animation frame.
    /// </summary>
    public class PixelAvatarDrawable : IDrawable
    {
        public string AvatarType { get; set; } = "default";
        public int Frame { get; set; } = 0;

        private const int SpriteW = 10;
        private const int SpriteH = 16;

        // ── Palette ────────────────────────────────────────────────────────────
        // '.' transparent   'H' hair     'F' flesh/skin   'E' eye
        // 'B' blue shirt    'R' red shirt  'G' green shirt  'V' violet shirt
        // 'P' pants         'K' shoes
        private static readonly Dictionary<char, Color> Palette = new()
        {
            ['.'] = Colors.Transparent,
            ['H'] = Color.FromArgb("#3D2B1F"),
            ['F'] = Color.FromArgb("#F5CBA7"),
            ['E'] = Color.FromArgb("#1A1A1A"),
            ['B'] = Color.FromArgb("#2980B9"),
            ['R'] = Color.FromArgb("#E74C3C"),
            ['G'] = Color.FromArgb("#27AE60"),
            ['V'] = Color.FromArgb("#9B59B6"),
            ['P'] = Color.FromArgb("#1A3A5C"),
            ['K'] = Color.FromArgb("#111111"),
        };

        // ── Sprites  [avatarType][frame][row] ──────────────────────────────────
        // Each row string is exactly 10 characters wide.
        private static readonly Dictionary<string, string[][]> Sprites = new()
        {
            // ── DEFAULT (standing, static) ─────────────────────────────────────
            ["default"] = new[]
            {
                new[] // frame 0
                {
                    "...HHHH...",  // 0
                    "..HFFFFH..",  // 1
                    "..HFEFFH..",  // 2  E = eye
                    "..HFFFFH..",  // 3
                    "...HHHH...",  // 4
                    "..BBBBB...",  // 5  shirt
                    ".BBBBBBB..",  // 6  arms at sides
                    "..BBBBB...",  // 7
                    "..BBBBB...",  // 8
                    "..PPPPP...",  // 9  pants
                    "..PPPPP...",  // 10
                    "..PP.PP...",  // 11 legs split
                    "..PP.PP...",  // 12
                    "..PP.PP...",  // 13
                    "..KK.KK...",  // 14 shoes
                    "..KK.KK...",  // 15
                },
                new[] // frame 1  (same — static character)
                {
                    "...HHHH...",
                    "..HFFFFH..",
                    "..HFEFFH..",
                    "..HFFFFH..",
                    "...HHHH...",
                    "..BBBBB...",
                    ".BBBBBBB..",
                    "..BBBBB...",
                    "..BBBBB...",
                    "..PPPPP...",
                    "..PPPPP...",
                    "..PP.PP...",
                    "..PP.PP...",
                    "..PP.PP...",
                    "..KK.KK...",
                    "..KK.KK...",
                },
            },

            // ── CARDIO / RUNNER ────────────────────────────────────────────────
            ["cardio"] = new[]
            {
                new[] // frame 0  left arm out, right leg forward
                {
                    "...HHHH...",  // 0
                    "..HFFFFH..",  // 1
                    "..HFEFFH..",  // 2
                    "..HFFFFH..",  // 3
                    "...HHHH...",  // 4
                    "R..RRRR...",  // 5  left arm extends to col 0
                    "...RRRR...",  // 6
                    "...RRRR..R",  // 7  right arm trails to col 9
                    "...RRRR...",  // 8
                    "....PP....",  // 9  hip
                    "..PP......",  // 10 left leg stepping back-left
                    ".PP.......",  // 11
                    "KK........",  // 12 left shoe at far left
                    "......PP..",  // 13 right leg stepping forward-right
                    ".......PP.",  // 14
                    ".......KK.",  // 15 right shoe
                },
                new[] // frame 1  right arm out, left leg forward
                {
                    "...HHHH...",  // 0
                    "..HFFFFH..",  // 1
                    "..HFEFFH..",  // 2
                    "..HFFFFH..",  // 3
                    "...HHHH...",  // 4
                    "...RRRR..R",  // 5  right arm forward
                    "...RRRR...",  // 6
                    "R..RRRR...",  // 7  left arm trails
                    "...RRRR...",  // 8
                    "....PP....",  // 9  hip
                    "......PP..",  // 10 right leg stepping back-right
                    ".......PP.",  // 11
                    ".......KK.",  // 12 right shoe at far right
                    "..PP......",  // 13 left leg stepping forward-left
                    ".PP.......",  // 14
                    ".KK.......",  // 15 left shoe
                },
            },

            // ── STRENGTH / LIFTER ──────────────────────────────────────────────
            ["strength"] = new[]
            {
                new[] // frame 0  arms down, holding dumbbells
                {
                    "...HHHH...",  // 0
                    "..HFFFFH..",  // 1
                    "..HFEFFH..",  // 2
                    "..HFFFFH..",  // 3
                    "...HHHH...",  // 4
                    "..GGGGG...",  // 5  shirt
                    ".GGGGGGG..",  // 6  arms at sides
                    "G.GGGGG.G.",  // 7  fists at cols 0 and 8
                    "GG.....GG.",  // 8  dumbbell plates extended
                    "..PPPPP...",  // 9
                    "..PPPPP...",  // 10
                    "..PP.PP...",  // 11
                    "..PP.PP...",  // 12
                    "..PP.PP...",  // 13
                    "..KK.KK...",  // 14
                    "..KK.KK...",  // 15
                },
                new[] // frame 1  arms raised overhead with dumbbells
                {
                    "G..HHHH..G",  // 0  dumbbells flanking head
                    "G.HFFFFHG.",  // 1  arms rising
                    ".GHFEFFHG.",  // 2
                    ".GHFFFFHG.",  // 3
                    "G..HHHH..G",  // 4
                    "...GGGGG..",  // 5  shirt (arms tucked in)
                    "...GGGGG..",  // 6
                    "...GGGGG..",  // 7
                    "..PPPPP...",  // 8
                    "..PPPPP...",  // 9
                    "..PP.PP...",  // 10
                    "..PP.PP...",  // 11
                    "..PP.PP...",  // 12
                    "..KK.KK...",  // 13
                    "..........",  // 14  (body ends higher when arms are up)
                    "..........",  // 15
                },
            },

            // ── FLEXIBILITY / YOGI ────────────────────────────────────────────
            ["flexibility"] = new[]
            {
                new[] // frame 0  T-pose (arms fully spread)
                {
                    "...HHHH...",  // 0
                    "..HFFFFH..",  // 1
                    "..HFEFFH..",  // 2
                    "..HFFFFH..",  // 3
                    "...HHHH...",  // 4
                    "VVVVVVVVVV",  // 5  arms fully spread = full width
                    "..VVVVVV..",  // 6
                    "..VVVVVV..",  // 7
                    "...PPPP...",  // 8
                    "...PPPP...",  // 9
                    "...PP.PP..",  // 10 legs split
                    "...PP.PP..",  // 11
                    "...PP.PP..",  // 12
                    "...PP.PP..",  // 13
                    "...KK.KK..",  // 14
                    "...KK.KK..",  // 15
                },
                new[] // frame 1  arms raised, wide warrior stance
                {
                    ".V.HHHH.V.",  // 0  arms starting to raise
                    ".VHFFFFHV.",  // 1
                    "..HFEFFH..",  // 2
                    "..HFFFFH..",  // 3
                    ".V.HHHH.V.",  // 4
                    "...VVVVV..",  // 5
                    "...VVVVV..",  // 6
                    "...VVVVV..",  // 7
                    "...PPPP...",  // 8
                    "PP.....PP.",  // 9  wide stance
                    "P.......P.",  // 10
                    "PP.....PP.",  // 11
                    "PP.....PP.",  // 12
                    "KK.....KK.",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
            },
        };

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var key = Sprites.ContainsKey(AvatarType) ? AvatarType : "default";
            var frames = Sprites[key];
            var sprite = frames[Frame % frames.Length];

            // Scale each sprite pixel to fill the available canvas evenly
            float pw = dirtyRect.Width / SpriteW;
            float ph = dirtyRect.Height / SpriteH;

            for (int row = 0; row < sprite.Length && row < SpriteH; row++)
            {
                var line = sprite[row];
                for (int col = 0; col < line.Length && col < SpriteW; col++)
                {
                    var ch = line[col];
                    if (ch == '.') continue;

                    if (Palette.TryGetValue(ch, out var color))
                    {
                        canvas.FillColor = color;
                        canvas.FillRectangle(
                            dirtyRect.X + col * pw,
                            dirtyRect.Y + row * ph,
                            pw, ph);
                    }
                }
            }
        }
    }
}
