namespace MDRNJfitness.Helpers.Sprites
{
    /// <summary>
    /// Placeholder dungeon art: 16×16 monsters (drawn at 2× next to the 32×32 hero)
    /// and a gold coin. Ids match MDRNJ.Shared.Game.DungeonRules.Monsters.
    /// To replace one, keep the 16×16 grid ('.' = transparent) and the id.
    /// </summary>
    public static class DungeonSprites
    {
        public const int MonsterSize = 16;

        private static readonly Dictionary<string, SpriteLayer> Monsters = new(StringComparer.OrdinalIgnoreCase)
        {
            // 'O' outline  'G'/'g'/'L' body / shade / shine  'E' eye  'W' eye shine  'M' mouth
            ["slime"] = new SpriteLayer(new[]
            {
                "................", // 0
                "................", // 1
                "................", // 2
                "................", // 3
                "................", // 4
                "......OOOO......", // 5
                "....OOGGGGOO....", // 6
                "...OGGLLGGGGO...", // 7
                "..OGGLLGGGGGGO..", // 8
                "..OGGGGGGGGGGO..", // 9
                ".OGGEWGGGGEWGGO.", // 10
                ".OGGEEGGGGEEGGO.", // 11
                ".OGGGGGMMGGGGGO.", // 12
                ".OgGGGGGGGGGGgO.", // 13
                "..OggggggggggO..", // 14
                "...OOOOOOOOOO...", // 15
            }, new Dictionary<char, Color>
            {
                ['O'] = Color.FromArgb("#0B1A10"),
                ['G'] = Color.FromArgb("#5BD66B"),
                ['g'] = Color.FromArgb("#2F8F45"),
                ['L'] = Color.FromArgb("#B8F7B0"),
                ['E'] = Color.FromArgb("#0B1A10"),
                ['W'] = Color.FromArgb("#FFFFFF"),
                ['M'] = Color.FromArgb("#1E5C2C"),
            }),

            // 'O' outline  'P' wing / body  'E' eye  'W' fang
            ["bat"] = new SpriteLayer(new[]
            {
                "................", // 0
                "................", // 1
                "................", // 2
                "................", // 3
                "O..............O", // 4
                "OO....O..O....OO", // 5
                "OPO..OPOOPO..OPO", // 6
                "OPPOOPPPPPPOOPPO", // 7
                "OPPPPPEPPEPPPPPO", // 8
                ".OPPPPPPPPPPPPO.", // 9
                "..OPPOPWWPOPPO..", // 10
                "...OO.OPPO.OO...", // 11
                ".......OO.......", // 12
                "................", // 13
                "................", // 14
                "................", // 15
            }, new Dictionary<char, Color>
            {
                ['O'] = Color.FromArgb("#140B24"),
                ['P'] = Color.FromArgb("#7A55C9"),
                ['E'] = Color.FromArgb("#FF4D4D"),
                ['W'] = Color.FromArgb("#FFFFFF"),
            }),

            // 'O' outline  'B'/'b' bone / shade  'E' eye socket
            ["skeleton"] = new SpriteLayer(new[]
            {
                "......OOOO......", // 0
                ".....OBBBBO.....", // 1
                "....OBBBBBBO....", // 2
                "....OBEBBEBO....", // 3
                "....OBEBBEBO....", // 4
                "....OBBBBBBO....", // 5
                ".....OBbbBO.....", // 6
                "......OBBO......", // 7
                "....OOBBBBOO....", // 8
                "...OB.BbbB.BO...", // 9
                "...OB.BbbB.BO...", // 10
                "....O.BBBB.O....", // 11
                "......B..B......", // 12
                "......B..B......", // 13
                ".....BB..BB.....", // 14
                "................", // 15
            }, new Dictionary<char, Color>
            {
                ['O'] = Color.FromArgb("#1A1028"),
                ['B'] = Color.FromArgb("#ECE9F7"),
                ['b'] = Color.FromArgb("#A9A3C7"),
                ['E'] = Color.FromArgb("#1A1028"),
            }),

            // 'O' outline  'H' horn  'R'/'r' skin / shade  'Y' eye  'W' tooth
            ["demon"] = new SpriteLayer(new[]
            {
                "..O..........O..", // 0
                "..OO........OO..", // 1
                "...OH......HO...", // 2
                "...OHOOOOOOHO...", // 3
                "..ORRRRRRRRRRO..", // 4
                "..ORRYRRRRYRRO..", // 5
                "..ORRRRRRRRRRO..", // 6
                "..ORRWRWRWRRRO..", // 7
                "...ORRRRRRRRO...", // 8
                ".OORRRRRRRRRROO.", // 9
                "ORrRRRRRRRRRRrRO", // 10
                "OrO.ORRRRRRO.OrO", // 11
                "....ORRRRRRO....", // 12
                "....ORRO.ORRO...", // 13
                "...ORRRO.ORRRO..", // 14
                "...OOOOO.OOOOO..", // 15
            }, new Dictionary<char, Color>
            {
                ['O'] = Color.FromArgb("#1A0808"),
                ['H'] = Color.FromArgb("#ECE9F7"),
                ['R'] = Color.FromArgb("#D8423A"),
                ['r'] = Color.FromArgb("#8E2222"),
                ['Y'] = Color.FromArgb("#FFC94A"),
                ['W'] = Color.FromArgb("#FFFFFF"),
            }),
        };

        /// <summary>'O' outline  'Y'/'y' gold / shade  'W' shine.</summary>
        public static readonly SpriteLayer Coin = new(new[]
        {
            ".OOOO.",
            "OYYYYO",
            "OYWYYO",
            "OYYYyO",
            "OYYyyO",
            ".OOOO.",
        }, new Dictionary<char, Color>
        {
            ['O'] = Color.FromArgb("#1B1406"),
            ['Y'] = Color.FromArgb("#FFC94A"),
            ['y'] = Color.FromArgb("#C98F1E"),
            ['W'] = Color.FromArgb("#FFF3C4"),
        });

        public static SpriteLayer Monster(string? id) =>
            id is not null && Monsters.TryGetValue(id, out var layer) ? layer : Monsters["slime"];
    }
}
