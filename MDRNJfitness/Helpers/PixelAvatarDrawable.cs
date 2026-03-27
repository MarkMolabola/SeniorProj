using Microsoft.Maui.Graphics;

namespace MDRNJfitness.Helpers
{
 
    /// Renders a 10×16 Kirby-style circular pixel-art character scaled to fill the canvas.
    /// Set AvatarType to "default", "cardio", "strength", or "flexibility".
    /// Set Frame to 0 or 1 to select the animation frame.
    
    public class PixelAvatarDrawable : IDrawable
    {
        public string AvatarType { get; set; } = "default";
        public int Frame { get; set; } = 0;

        //this is the avatar pixel dimensions : currently 10x16
        private const int SpriteW = 10; 
        private const int SpriteH = 16;

        // ── Palette ─────────────────────────────────────────────────────────
        // '.' transparent   'P' pink body    'E' dark eye     'W' eye shine
        // 'C' cheek blush   'M' mouth        'R' red          'B' blue
        // 'G' green         'Y' yellow       'V' violet
        private static readonly Dictionary<char, Color> Palette = new()
        {
            ['.'] = Colors.Transparent,
            ['P'] = Color.FromArgb("#FFB7D5"),  // pink body
            ['E'] = Color.FromArgb("#1A1A2E"),  // eye dark
            ['W'] = Color.FromArgb("#FFFFFF"),  // eye shine
            ['C'] = Color.FromArgb("#FF6BAE"),  // cheek blush
            ['M'] = Color.FromArgb("#CC3366"),  // mouth
            ['R'] = Color.FromArgb("#E74C3C"),  // red (headband)
            ['B'] = Color.FromArgb("#2980B9"),  // blue (gloves)
            ['G'] = Color.FromArgb("#27AE60"),  // green
            ['Y'] = Color.FromArgb("#F1C40F"),  // yellow (star)
            ['V'] = Color.FromArgb("#9B59B6"),  // violet (yoga)
        };

        // ── Sprites  [avatarType][frame][row] ──────────────────────────────
        // Each row string is exactly 10 characters wide.
        // Round body occupies rows 2-10; accessories use rows 0-1; feet use rows 11-13.
        private static readonly Dictionary<string, string[][]> Sprites = new()
        {
            // ── DEFAULT (Kirby idle with star gleam) ────────────────────────
            ["default"] = new[]
            {
                new[] // frame 0 – neutral face
                {
                    "....YY....",  // 0  star glint
                    "...YYYY...",  // 1  star
                    "..PPPPPP..",  // 2  body top
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5  eyes
                    "PP.EW.EW.P",  // 6  eye shine
                    "PPC.....CP",  // 7  cheeks
                    "PP..MM..PP",  // 8  mouth
                    ".PPPPPPPP.",  // 9  body bottom
                    "..PPPPPP..",  // 10
                    "..PP..PP..",  // 11 feet
                    "..PP..PP..",  // 12
                    "..PP..PP..",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
                new[] // frame 1 – happy bounce (wider smile, feet hop)
                {
                    "....YY....",  // 0
                    "...YYYY...",  // 1
                    "..PPPPPP..",  // 2
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5
                    "PP.EW.EW.P",  // 6
                    "PPC.....CP",  // 7
                    "PP.MMMM.PP",  // 8  wider smile
                    ".PPPPPPPP.",  // 9
                    "..PPPPPP..",  // 10
                    "...PP.PP..",  // 11 feet together (hop)
                    "...PP.PP..",  // 12
                    "..PP..PP..",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
            },

            // ── CARDIO───────────────────────────
            ["cardio"] = new[]
            {
                new[] // frame 0 – left foot forward
                {
                    "..........",  // 0
                    "..RRRRRR..",  // 1  headband across forehead
                    "..PPPPPP..",  // 2  body top
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5  determined eyes (no shine)
                    "PP.EE.EE.P",  // 6
                    "PPC.....CP",  // 7  cheeks
                    "PP..MM..PP",  // 8  mouth
                    ".PPPPPPPP.",  // 9  body bottom
                    "..PPPPPP..",  // 10
                    "PPP....PP.",  // 11 left foot fwd (cols 0-2), right foot back (7-8)
                    "PPP.....P.",  // 12
                    ".PP.......",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
                new[] // frame 1 – right foot forward
                {
                    "..........",  // 0
                    "..RRRRRR..",  // 1  headband
                    "..PPPPPP..",  // 2
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5
                    "PP.EE.EE.P",  // 6
                    "PPC.....CP",  // 7
                    "PP..MM..PP",  // 8
                    ".PPPPPPPP.",  // 9
                    "..PPPPPP..",  // 10
                    ".PP....PPP",  // 11 left foot back (cols 1-2), right foot fwd (7-9)
                    ".P.....PPP",  // 12
                    ".......PP.",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
            },

            // ── STRENGTH  ───────────────────────────
            ["strength"] = new[]
            {
                new[] // frame 0 – gloves lowered at sides
                {
                    "..........",  // 0
                    "..........",  // 1
                    "..PPPPPP..",  // 2  body top
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5  eyes
                    "PP.EW.EW.P",  // 6  eye shine
                    "PPC.....CP",  // 7  cheeks
                    "PP..MM..PP",  // 8  mouth
                    "BPPPPPPPPB",  // 9  gloves at lower body sides
                    "BB.PPPP.BB",  // 10 gloves below body
                    "..PP..PP..",  // 11 feet
                    "..PP..PP..",  // 12
                    "..........",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
                new[] // frame 1 – gloves raised overhead
                {
                    ".BB....BB.",  // 0  gloves raised
                    ".BB....BB.",  // 1
                    "..PPPPPP..",  // 2  body top
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5
                    "PP.EW.EW.P",  // 6
                    "PPC.....CP",  // 7
                    "PP..MM..PP",  // 8
                    ".PPPPPPPP.",  // 9
                    "..PPPPPP..",  // 10
                    "..PP..PP..",  // 11 feet
                    "..PP..PP..",  // 12
                    "..........",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
            },

            // ── FLEXIBILITY  ─────────────────────────
            ["flexibility"] = new[]
            {
                new[] // frame 0 – arms spread wide (T-pose)
                {
                    "..........",  // 0
                    "..........",  // 1
                    "..PPPPPP..",  // 2  body top
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5  eyes
                    "PP.EW.EW.P",  // 6  eye shine
                    "PPC.....CP",  // 7  cheeks
                    "PP..MM..PP",  // 8  mouth
                    "VPPPPPPPPV",  // 9  violet arms at body sides
                    "VV.PPPP.VV",  // 10 arms extended wide
                    "..PP..PP..",  // 11 feet
                    "..PP..PP..",  // 12
                    "..........",  // 13
                    "..........",  // 14
                    "..........",  // 15
                },
                new[] // frame 1 – arms raised overhead (warrior pose)
                {
                    ".VV....VV.",  // 0  arms raised
                    ".VV....VV.",  // 1
                    "..PPPPPP..",  // 2  body top
                    ".PPPPPPPP.",  // 3
                    "PPPPPPPPPP",  // 4
                    "PP.EE.EE.P",  // 5
                    "PP.EW.EW.P",  // 6
                    "PPC.....CP",  // 7
                    "PP..MM..PP",  // 8
                    ".PPPPPPPP.",  // 9
                    "..PPPPPP..",  // 10
                    "VP......PV",  // 11 wide stance legs
                    "VP......PV",  // 12
                    ".V......V.",  // 13
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
