namespace MDRNJfitness.Helpers.Sprites
{
    /// <summary>
    /// 32×32 layered hero. Drawn as Body, then the outfit for the character's
    /// OutfitSlot on top. To add an outfit, draw it on the same 32×32 grid
    /// (only the outfit's own pixels; everything else '.') and register it in
    /// <see cref="Outfits"/>.
    /// </summary>
    public static class HeroSprites
    {
        public const int Size = 32;

        /// <summary>
        /// Idle frame 1 "breathes": rows above this index sink 1px while the
        /// legs (this row and below) stay planted.
        /// </summary>
        public const int BreathRow = 23;

        // ── Palette ─────────────────────────────────────────────────────────
        // 'O' outline     'H'/'h' hair / highlight   'R'/'r' headband / shade
        // 'S'/'s'/'L' skin / shade / light   'E' eye   'W' eye shine   'M' mouth
        // Outfit: 'o' outline   'T'/'t'/'U' shirt / shade / highlight
        //         'B'/'b' shorts / shade   'K'/'k' sneakers / sole
        private static readonly Dictionary<char, Color> Palette = new()
        {
            ['O'] = Color.FromArgb("#1A1028"),
            ['H'] = Color.FromArgb("#3B2A1F"),
            ['h'] = Color.FromArgb("#5A4130"),
            ['R'] = Color.FromArgb("#E2564A"),
            ['r'] = Color.FromArgb("#A8352D"),
            ['S'] = Color.FromArgb("#F1B68A"),
            ['s'] = Color.FromArgb("#C9825A"),
            ['L'] = Color.FromArgb("#FFD3AE"),
            ['E'] = Color.FromArgb("#1A1028"),
            ['W'] = Color.FromArgb("#FFFFFF"),
            ['M'] = Color.FromArgb("#8A3B3B"),
            ['o'] = Color.FromArgb("#1A1028"),
            ['T'] = Color.FromArgb("#3FD6C3"),
            ['t'] = Color.FromArgb("#23968A"),
            ['U'] = Color.FromArgb("#8AF0E3"),
            ['B'] = Color.FromArgb("#2B3A67"),
            ['b'] = Color.FromArgb("#1E2A4D"),
            ['K'] = Color.FromArgb("#ECE9F7"),
            ['k'] = Color.FromArgb("#A9A3C7"),
            // Cardio: 'G'/'g'/'Y' tee / shade / highlight   'C'/'c' shorts / shade
            ['G'] = Color.FromArgb("#A6E04B"),
            ['g'] = Color.FromArgb("#6FA62E"),
            ['Y'] = Color.FromArgb("#D6F78F"),
            ['C'] = Color.FromArgb("#4A7FD4"),
            ['c'] = Color.FromArgb("#2F5AA0"),
            // Strength: 'A'/'a'/'V' tank / shade / highlight   'D'/'d' shorts / shade
            ['A'] = Color.FromArgb("#E8603C"),
            ['a'] = Color.FromArgb("#A63E28"),
            ['V'] = Color.FromArgb("#FF9070"),
            ['D'] = Color.FromArgb("#3A3548"),
            ['d'] = Color.FromArgb("#26222F"),
            // Flexibility: 'P'/'p'/'Q' top / shade / highlight   'N'/'n' leggings / shade
            ['P'] = Color.FromArgb("#9B7BE8"),
            ['p'] = Color.FromArgb("#6C4FC0"),
            ['Q'] = Color.FromArgb("#C7B3FF"),
            ['N'] = Color.FromArgb("#3A2F66"),
            ['n'] = Color.FromArgb("#282050"),
        };

        // Base body: head, headband, skin. Outfits are drawn over it.
        private static readonly string[] BodyRows =
        {
            "..............OO................", // 0
            "............OOOHHOOO............", // 1
            "..........OOHHHHHHhhOO..........", // 2
            ".........OHHHHHHHHHHhhO.........", // 3
            ".........OHHhHHHHHHHHHO.........", // 4
            ".........ORRRRRRRRRRRrO.........", // 5
            ".........ORRRRRRRRRRrrOOO.......", // 6
            ".........OSSSSSSSSSSSsOrrO......", // 7
            ".........OSSWESSSSWESsO.OrO.....", // 8
            ".........OSSEESSSSEESsO..O......", // 9
            ".........OLSSSSSSSSSSsO.........", // 10
            ".........OSSSSMSSMSSssO.........", // 11
            "..........OSSSSMMSSSsO..........", // 12
            "...........OOSSSSSssOO..........", // 13
            ".............OSSSsO.............", // 14
            ".........OSSSSSSSSSSSSO.........", // 15
            ".......OOSSSSSSSSSSSSSSOO.......", // 16
            "......OSSSSSSSSSSSSSSSSsSO......", // 17
            "......OSSsSSSSSSSSSSSSsSSO......", // 18
            "......OSSSSSSSSSSSSSSSSSSO......", // 19
            "......OSSsSSSSSSSSSSSSSSsO......", // 20
            "......OSSsSSSSSSSSSSsSSSsO......", // 21
            "......OSSsSsSSSSSSSSsSSSsO......", // 22
            "......OSSsSSSSSSSSSSSSSSsO......", // 23
            ".....OSSSsSSSSSSSSSSSSSSSsO.....", // 24
            ".....OSSssSSSSSssSSSSSSSssO.....", // 25
            "......OOOOSSSSSSSSSSSSOOOO......", // 26
            "..........OSSSsOOSSSsO..........", // 27
            "..........OSSSsOOSSSsO..........", // 28
            ".........OSSSSsOOSSSSsO.........", // 29
            ".........OsssssOOsssssO.........", // 30
            "..........OOOOO..OOOOO..........", // 31
        };

        // Default outfit: teal training tee, navy shorts, white sneakers.
        private static readonly string[] DefaultOutfitRows =
        {
            "................................", // 0
            "................................", // 1
            "................................", // 2
            "................................", // 3
            "................................", // 4
            "................................", // 5
            "................................", // 6
            "................................", // 7
            "................................", // 8
            "................................", // 9
            "................................", // 10
            "................................", // 11
            "................................", // 12
            "................................", // 13
            "................................", // 14
            ".........oTTTo....oTTTo.........", // 15
            ".......ooTTTTTTooTTTTTToo.......", // 16
            "......oTTUTTTTTTTTTTTTTtTo......", // 17
            "......oTTtoTTTTTTTTTTotTTo......", // 18
            "......oooooTTTTTTTTTTooooo......", // 19
            "..........oTTTTTTTTTTo..........", // 20
            "..........oTTTTTTTTTto..........", // 21
            "..........otTTTTTTTTto..........", // 22
            "..........oooooooooooo..........", // 23
            "..........oBBBBBBBBBBo..........", // 24
            "..........oBBBBbbBBBBo..........", // 25
            "..........oBBBBooBBBBo..........", // 26
            "................................", // 27
            "...........KKKk..KKKk...........", // 28
            "..........KKKKk..KKKKk..........", // 29
            "..........kkkkk..kkkkk..........", // 30
            "................................", // 31
        };

        // Cardio outfit: lime running tee, blue shorts, white sneakers.
        // Strength outfit: orange tank top, charcoal shorts, white sneakers.
        // Flexibility outfit: lavender long-sleeve top, leggings, barefoot.
        private static readonly string[] CardioOutfitRows =
        {
            "................................", // 0
            "................................", // 1
            "................................", // 2
            "................................", // 3
            "................................", // 4
            "................................", // 5
            "................................", // 6
            "................................", // 7
            "................................", // 8
            "................................", // 9
            "................................", // 10
            "................................", // 11
            "................................", // 12
            "................................", // 13
            "................................", // 14
            ".........oGGGo....oGGGo.........", // 15
            ".......ooGGGGGGooGGGGGGoo.......", // 16
            "......oGGYGGGGGGGGGGGGGgGo......", // 17
            "......oGGgoGGGGGGGGGGogGGo......", // 18
            "......oooooGGGGGGGGGGooooo......", // 19
            "..........oGGGGGGGGGGo..........", // 20
            "..........oGGGGGGGGGgo..........", // 21
            "..........ogGGGGGGGGgo..........", // 22
            "..........oooooooooooo..........", // 23
            "..........oCCCCCCCCCCo..........", // 24
            "..........oCCCCccCCCCo..........", // 25
            "..........oCCCCooCCCCo..........", // 26
            "................................", // 27
            "...........KKKk..KKKk...........", // 28
            "..........KKKKk..KKKKk..........", // 29
            "..........kkkkk..kkkkk..........", // 30
            "................................", // 31
        };

        private static readonly string[] StrengthOutfitRows =
        {
            "................................", // 0
            "................................", // 1
            "................................", // 2
            "................................", // 3
            "................................", // 4
            "................................", // 5
            "................................", // 6
            "................................", // 7
            "................................", // 8
            "................................", // 9
            "................................", // 10
            "................................", // 11
            "................................", // 12
            "................................", // 13
            "................................", // 14
            "..........oAo......oAo..........", // 15
            "..........oAAooooooAAo..........", // 16
            "..........oAVAAAAAAAao..........", // 17
            "..........oAAAAAAAAAao..........", // 18
            "..........oAAAAAAAAAao..........", // 19
            "..........oAAAAAAAAAAo..........", // 20
            "..........oAAAAAAAAAao..........", // 21
            "..........oaAAAAAAAAao..........", // 22
            "..........oooooooooooo..........", // 23
            "..........oDDDDDDDDDDo..........", // 24
            "..........oDDDDddDDDDo..........", // 25
            "..........oDDDDooDDDDo..........", // 26
            "................................", // 27
            "...........KKKk..KKKk...........", // 28
            "..........KKKKk..KKKKk..........", // 29
            "..........kkkkk..kkkkk..........", // 30
            "................................", // 31
        };

        private static readonly string[] FlexibilityOutfitRows =
        {
            "................................", // 0
            "................................", // 1
            "................................", // 2
            "................................", // 3
            "................................", // 4
            "................................", // 5
            "................................", // 6
            "................................", // 7
            "................................", // 8
            "................................", // 9
            "................................", // 10
            "................................", // 11
            "................................", // 12
            "................................", // 13
            "................................", // 14
            ".........oPPPo....oPPPo.........", // 15
            ".......ooPPPPPPooPPPPPPoo.......", // 16
            "......oPPQPPPPPPPPPPPPPpPo......", // 17
            "......oPPpoPPPPPPPPPPopPPo......", // 18
            "......oPPpoPPPPPPPPPPopPPo......", // 19
            "......oPPpoPPPPPPPPPPopPPo......", // 20
            "......oPPpoPPPPPPPPPpopPPo......", // 21
            "......oooooPPPPPPPPPpooooo......", // 22
            "..........oooooooooooo..........", // 23
            "..........oNNNNNNNNNNo..........", // 24
            "..........oNNNNnnNNNNo..........", // 25
            "..........oNNNNooNNNNo..........", // 26
            "..........oNNNnooNNNno..........", // 27
            "..........oNNNnooNNNno..........", // 28
            "................................", // 29
            "................................", // 30
            "................................", // 31
        };

        public static readonly SpriteLayer Body = new(BodyRows, Palette);

        /// <summary>Outfit layers keyed by CharacterState.OutfitSlot.</summary>
        private static readonly Dictionary<string, SpriteLayer> Outfits = new(StringComparer.OrdinalIgnoreCase)
        {
            ["default"] = new SpriteLayer(DefaultOutfitRows, Palette),
            ["cardio"] = new SpriteLayer(CardioOutfitRows, Palette),
            ["strength"] = new SpriteLayer(StrengthOutfitRows, Palette),
            ["flexibility"] = new SpriteLayer(FlexibilityOutfitRows, Palette),
        };

        public static bool TryGetOutfit(string? outfitSlot, out SpriteLayer outfit)
            => Outfits.TryGetValue(string.IsNullOrWhiteSpace(outfitSlot) ? "default" : outfitSlot, out outfit!);
    }
}
