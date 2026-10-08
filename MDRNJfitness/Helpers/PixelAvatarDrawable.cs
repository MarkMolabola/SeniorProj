using MDRNJfitness.Helpers.Sprites;
using Microsoft.Maui.Graphics;

namespace MDRNJfitness.Helpers
{
    /*
 
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
    */
    /// Renders the pixel-art character, scaled by a whole number so pixels stay
    /// crisp, centred horizontally and standing on the bottom of the canvas.
    /// Set AvatarType to the character's OutfitSlot ("default", "cardio", ...).
    /// Set Frame to 0 or 1 to select the animation frame.
    ///
    /// Outfits registered in HeroSprites ("default", "cardio", "strength",
    /// "flexibility") draw the layered 32×32 hero; unknown types draw the
    /// default hero. The 10×16 stick figures below are kept as a fallback for
    /// any type that has a stick figure but no hero outfit:
    ///   default     → pastel purple, front-facing, arms at sides
    ///   cardio      → pastel green,  side-profile running right
    ///   strength    → pastel red,    front-facing, overhead barbell press
    ///   flexibility → pastel blue,   front-facing, stretch poses

    public class PixelAvatarDrawable : IDrawable
    {
        public string AvatarType { get; set; } = "default";

        /// <summary>Chosen hero look (HeroAvatars id). Full-body avatars ignore AvatarType.</summary>
        public string AvatarId { get; set; } = "classic";
        public int Frame { get; set; } = 0;

        private const int SpriteW = 10;
        private const int SpriteH = 16;

        // ── Palette ─────────────────────────────────────────────────────────
        // '.' transparent
        // 'P' purple   'R' red   'G' green   'B' blue
        // 'E' black  — used for eyes and barbell
        private static readonly Dictionary<char, Color> Palette = new()
        {
            ['.'] = Colors.Transparent,
            ['P'] = Color.FromArgb("#C9A6E8"),  // purple
            ['R'] = Color.FromArgb("#F4AAAA"),  // red
            ['G'] = Color.FromArgb("#A0E4A0"),  // green
            ['B'] = Color.FromArgb("#A0C8F4"),  // blue
            ['E'] = Color.FromArgb("#2C2C4A"),  // black — eyes + barbell
        };

        private static readonly Dictionary<string, string[][]> Sprites = new()
        {
            // ── DEFAULT  pastel purple, front-facing, arms hang at sides ───
            ["default"] = new[]
            {
            new[] // frame 0 - neutral stand
            {
                "..PPPPPP..",  // 0  head
                "..PE..EP..",  // 1  eyes (cols 3 & 6)
                "..PPPPPP..",  // 2  head
                "....PP....",  // 3  neck
                "..PPPPPP..",  // 4  shoulders (wider than torso)
                ".P..PP..P.",  // 5  arms at sides (col 1 & 8) + body (cols 4-5)
                ".P..PP..P.",  // 6
                ".P..PP..P.",  // 7
                "....PP....",  // 8  lower torso
                "...PPPP...",  // 9  hips
                "..PP..PP..",  // 10 legs
                "..PP..PP..",  // 11
                ".PP....PP.",  // 12 feet
                "..........",  // 13
                "..........",  // 14
                "..........",  // 15
            },
            new[] // frame 1 - slight weight shift (feet vary, arms same)
            {
                "..PPPPPP..",  // 0
                "..PE..EP..",  // 1
                "..PPPPPP..",  // 2
                "....PP....",  // 3
                "..PPPPPP..",  // 4
                ".P..PP..P.",  // 5
                ".P..PP..P.",  // 6
                ".P..PP..P.",  // 7
                "....PP....",  // 8
                "...PPPP...",  // 9
                "...PP.PP..",  // 10 slight weight shift right
                "...PP.PP..",  // 11
                "..PP..PPP.",  // 12 right foot planted
                "..........",  // 13
                "..........",  // 14
                "..........",  // 15
            },
        },

            // ── CARDIO  pastel green, side-profile running right ───────────
            // Body centre: cols 5-6.  Head: cols 4-7 (side-on, 4 px wide).
            // Arms alternate: one forward (cols 8-9), one behind (cols 0-2).
            // Legs stride: forward foot reaches cols 8-9, trailing foot cols 0-1.
            ["cardio"] = new[]
            {
            new[] // frame 0 - right foot forward, back arm high
            {
                "....GGGG..",  // 0  head (cols 4-7)
                "....G.EG..",  // 1  single eye col 6 (facing right)
                "....GGGG..",  // 2  head
                ".....GG...",  // 3  neck (cols 5-6, slight forward lean)
                "....GGGG..",  // 4  shoulders (cols 4-7)
                "..GG.GG.G.",  // 5  back/trailing arm high (cols 2-3) + body
                ".....GG.G.",  // 6  body
                ".....GG.GG",  // 7  body + forward arm low (cols 8-9)
                ".....GG...",  // 8  body
                "....GGGG..",  // 9  hips (cols 4-7)
                "...GG.GGG.",  // 10 trailing thigh (3-4), lead thigh (6-8)
                ".GG....GG.",  // 11 trailing knee (1-2), lead knee (7-8)
                "GG......GG",  // 12 trailing foot (0-1), lead foot (8-9)
                "..........",  // 13
                "..........",  // 14
                "..........",  // 15
            },
            new[] // frame 1 - left foot forward, forward arm high (opposite stride)
            {
                "....GGGG..",  // 0
                "....G.EG..",  // 1
                "....GGGG..",  // 2
                ".....GG...",  // 3
                "...GGGGG..",  // 4
                "..G..GG.GG",  // 5  forward arm high (cols 8-9) + body
                "..G..GG...",  // 6
                "GG...GG...",  // 7  trailing arm low (cols 0-1) + body
                ".....GG...",  // 8
                "....GGGG..",  // 9
                ".GGG..GGG.",  // 10 lead thigh (1-3), trailing thigh (6-8)
                "GG.....GG.",  // 11 lead foot up (0-1), trailing knee (7-8)
                "G......GGG",  // 12 lead foot (0), trailing foot (7-9)
                "..........",  // 13
                "..........",  // 14
                "..........",  // 15
            },
        },

            // ── STRENGTH  pastel red, front-facing, overhead barbell press ─
            // Barbell drawn in 'E' (dark/black) spanning the full 10-col width.
            // Frame 0: bar at chin/shoulder height (start of press).
            // Frame 1: bar locked out overhead, arms extended up (rows 1-2),
            //          head shifts down to rows 3-5 to make room.
            ["strength"] = new[]
            {
            new[] // frame 0 - bar at chin level, ready to press
            {
                "..RRRRRR..",  // 0  head
                "..RE..ER..",  // 1  eyes (cols 3 & 6)
                "..RRRRRR..",  // 2  head
                "....RR....",  // 3  neck
                "EEEEEEEEEE",  // 4  BARBELL at shoulder height (full width)
                ".R..RR..R.",  // 5  body (hands grip bar - same row)
                "....RR....",  // 6
                "....RR....",  // 7
                "....RR....",  // 8
                "...RRRR...",  // 9  hips
                "..RR..RR..",  // 10 legs
                "..RR..RR..",  // 11
                ".RR....RR.",  // 12 feet
                "..........",  // 13
                "..........",  // 14
                "..........",  // 15
            },
            new[] // frame 1 - bar locked out overhead, arms fully extended
            {
                "EEEEEEEEEE",  // 0  BARBELL overhead (full width)
                "...RR.RR..",  // 1  arms extended up (cols 3-4 & 6-7)
                "...RR.RR..",  // 2  arms
                "..RRRRRR..",  // 3  head (shifted down 3 rows)
                "..RE..ER..",  // 4  eyes
                "..RRRRRR..",  // 5  head
                "....RR....",  // 6  neck
                "....RR....",  // 7  body
                "....RR....",  // 8
                "....RR....",  // 9
                "...RRRR...",  // 10 hips
                "..RR..RR..",  // 11 legs
                "..RR..RR..",  // 12
                ".RR....RR.",  // 13 feet
                "..........",  // 14
                "..........",  // 15
            },
        },

            // ── FLEXIBILITY  pastel blue, front-facing stretch poses ───────
            ["flexibility"] = new[]
            {
            new[] // frame 0 - T-pose, arms fully spread at shoulder height
            {
                "..BBBBBB..",  // 0  head
                "..BE..EB..",  // 1  eyes (cols 3 & 6)
                "..BBBBBB..",  // 2  head
                "....BB....",  // 3  neck
                "BBBBBBBBBB",  // 4  arms fully spread (full width)
                "....BB....",  // 5  body
                "....BB....",  // 6
                "....BB....",  // 7
                "....BB....",  // 8
                "...BBBB...",  // 9  hips
                "..BB..BB..",  // 10 legs
                "..BB..BB..",  // 11
                ".BB....BB.",  // 12 feet
                "..........",  // 13
                "..........",  // 14
                "..........",  // 15
            },
            new[] // frame 1 - arms raised overhead, wide-stance legs
            {
                "BB......BB",  // 0  arms raised high
                ".BB....BB.",  // 1  arms angling in
                "..BBBBBB..",  // 2  head (shifted down 2 rows)
                "..BE..EB..",  // 3  eyes
                "..BBBBBB..",  // 4  head
                "....BB....",  // 5  neck
                "....BB....",  // 6  body
                "....BB....",  // 7
                "....BB....",  // 8
                "...BBBB...",  // 9  hips
                "BB......BB",  // 10 wide stance
                "BB......BB",  // 11
                ".B......B.",  // 12 feet wide
                "..........",  // 13
                "..........",  // 14
                "..........",  // 15
            },
        },
        };

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = false;

            if (HeroSprites.TryGetFullBodyAvatar(AvatarId, out var fullBody, out var breathRow))
            {
                DrawLayer(canvas, fullBody, Place(dirtyRect, HeroSprites.Size, HeroSprites.Size), Frame, breathRow);
                return;
            }

            // Layered 32×32 hero for registered outfits, and for any unknown type
            var useHero = HeroSprites.TryGetOutfit(AvatarType, out var outfit);
            if (!useHero && !Sprites.ContainsKey(AvatarType))
                useHero = HeroSprites.TryGetOutfit("default", out outfit);

            if (useHero)
            {
                var hero = Place(dirtyRect, HeroSprites.Size, HeroSprites.Size);
                DrawLayer(canvas, HeroSprites.Body, hero, Frame, HeroSprites.BreathRow);
                DrawLayer(canvas, outfit, hero, Frame, HeroSprites.BreathRow);
                return;
            }

            // Legacy 10×16 stick figures (frames are drawn per pose)
            var frames = Sprites[AvatarType];
            var sprite = frames[Frame % frames.Length];
            var legacy = Place(dirtyRect, SpriteW, SpriteH);

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
                            legacy.X + col * legacy.Scale,
                            legacy.Y + row * legacy.Scale,
                            legacy.Scale, legacy.Scale);
                    }
                }
            }
        }

        /// <summary>
        /// Largest whole-number scale that fits, centred horizontally and
        /// bottom-aligned (feet on the floor). Falls back to a fractional scale
        /// only when the canvas is smaller than the sprite.
        /// </summary>
        private static (float X, float Y, float Scale) Place(RectF area, int width, int height)
        {
            var fit = MathF.Min(area.Width / width, area.Height / height);
            var scale = fit >= 1 ? MathF.Floor(fit) : fit;
            return (
                area.X + MathF.Floor((area.Width - width * scale) / 2),
                area.Y + MathF.Floor(area.Height - height * scale),
                scale);
        }

        /// <summary>
        /// Draws one layer, merging same-colour runs into single rectangles.
        /// On odd frames rows above breathRow sink 1px (the row just above
        /// breathRow is dropped) so the character idles without extra art.
        /// </summary>
        private static void DrawLayer(ICanvas canvas, SpriteLayer layer, (float X, float Y, float Scale) at, int frame, int breathRow)
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

                var row = layer.Rows[y];
                for (int x = 0; x < row.Length;)
                {
                    var ch = row[x];
                    var end = x + 1;
                    while (end < row.Length && row[end] == ch) end++;

                    if (ch != '.' && layer.Palette.TryGetValue(ch, out var color))
                    {
                        canvas.FillColor = color;
                        canvas.FillRectangle(
                            at.X + x * at.Scale,
                            at.Y + destY * at.Scale,
                            (end - x) * at.Scale, at.Scale);
                    }
                    x = end;
                }
            }
        }
    }

}