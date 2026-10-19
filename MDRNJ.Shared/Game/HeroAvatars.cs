using System.Text.Json;

namespace MDRNJ.Shared.Game
{
    /// <summary>One piece of a special hero's built-in outfit, e.g. the Ninja's mask.</summary>
    /// <param name="Bonus">Stat points added to <paramref name="Stat"/> while the hero is selected.</param>
    public sealed record HeroPiece(GearSlot Slot, string Name, StatKind Stat, int Bonus);

    /// <summary>
    /// A hero look. The Classic Hero is free and wears shop gear. Special heroes are
    /// bought in the shop and come dressed: their <see cref="Pieces"/> fill the gear
    /// slots (no gear is drawn on them) and boost stats instead of gear.
    /// </summary>
    public sealed record HeroAvatar(string Id, string Name, GearRarity Rarity, int Price, IReadOnlyList<HeroPiece> Pieces)
    {
        public bool IsSpecial => Pieces.Count > 0;

        public HeroPiece? PieceFor(GearSlot slot) => Pieces.FirstOrDefault(p => p.Slot == slot);

        /// <summary>e.g. "+20 END · +10 FLX".</summary>
        public string BonusText => string.Join(" · ", Pieces
            .GroupBy(p => p.Stat)
            .OrderBy(g => g.Key)
            .Select(g => $"+{g.Sum(p => p.Bonus)} {GearStats.ShortName(g.Key)}"));
    }

    /// <summary>
    /// Hero looks a player can pick on the Hero tab. Shared so the API only accepts
    /// known ids and charges the right price, and the app can list them. Special
    /// heroes' names, prices and outfits come from heroes.jsonc (embedded in this
    /// assembly). To add one, add its sprite in the app (HeroSprites) and an entry there.
    /// </summary>
    public static class HeroAvatars
    {
        /// <summary>The layered hero whose outfit changes with training.</summary>
        public const string Classic = "classic";

        public static readonly IReadOnlyList<HeroAvatar> All =
            new[] { new HeroAvatar(Classic, "Classic Hero", GearRarity.Common, 0, Array.Empty<HeroPiece>()) }
                .Concat(LoadSpecialHeroes())
                .ToList();

        public static HeroAvatar? Find(string? id) => All.FirstOrDefault(a => a.Id == id);

        public static bool IsValid(string? id) => Find(id) is not null;

        /// <summary>True for heroes that come with their own outfit (everyone but the Classic Hero).</summary>
        public static bool IsSpecial(string? id) => Find(id)?.IsSpecial == true;

        // ── heroes.jsonc ────────────────────────────────────────────────────

        private const string BalanceFile = "MDRNJ.Shared.Game.heroes.jsonc";

        private sealed record HeroEntry(string Id, string Name, string Rarity, int Price, List<PieceEntry> Pieces);
        private sealed record PieceEntry(string Slot, string Name, string Stat, int Bonus);

        /// <summary>Reads and checks heroes.jsonc; a mistake fails loudly at startup instead of quietly mis-pricing.</summary>
        private static IEnumerable<HeroAvatar> LoadSpecialHeroes()
        {
            using var stream = typeof(HeroAvatars).Assembly.GetManifestResourceStream(BalanceFile)
                ?? throw new InvalidOperationException($"{BalanceFile} isn't embedded in MDRNJ.Shared.");
            var entries = JsonSerializer.Deserialize<List<HeroEntry>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            }) ?? new List<HeroEntry>();

            var heroes = new List<HeroAvatar>();
            foreach (var e in entries)
            {
                string Problem(string what) => $"heroes.jsonc, hero \"{e.Id}\": {what}";

                if (string.IsNullOrWhiteSpace(e.Id) || e.Id == Classic || heroes.Any(h => h.Id == e.Id))
                    throw new InvalidOperationException(Problem("needs a unique id (and not \"classic\")."));
                if (!Enum.TryParse<GearRarity>(e.Rarity, true, out var rarity))
                    throw new InvalidOperationException(Problem($"unknown rarity \"{e.Rarity}\" (Common, Rare, Epic, Legendary)."));
                if (e.Price < 0)
                    throw new InvalidOperationException(Problem("price can't be negative."));
                if (e.Pieces is not { Count: > 0 })
                    throw new InvalidOperationException(Problem("needs at least one piece."));

                var pieces = new List<HeroPiece>();
                foreach (var p in e.Pieces)
                {
                    if (!Enum.TryParse<GearSlot>(p.Slot, true, out var slot) || !Enum.IsDefined(slot))
                        throw new InvalidOperationException(Problem($"unknown slot \"{p.Slot}\" (Head, Top, Bottom, Shoes, Accessory)."));
                    if (pieces.Any(x => x.Slot == slot))
                        throw new InvalidOperationException(Problem($"has two {slot} pieces."));
                    var stat = p.Stat?.ToUpperInvariant() switch
                    {
                        "STR" => StatKind.Strength,
                        "END" => StatKind.Endurance,
                        "FLX" => StatKind.Flexibility,
                        _ => throw new InvalidOperationException(Problem($"unknown stat \"{p.Stat}\" on {p.Name} (STR, END, FLX)."))
                    };
                    if (p.Bonus < 0)
                        throw new InvalidOperationException(Problem($"{p.Name}'s bonus can't be negative."));
                    pieces.Add(new HeroPiece(slot, p.Name, stat, p.Bonus));
                }
                heroes.Add(new HeroAvatar(e.Id, e.Name, rarity, e.Price, pieces));
            }
            return heroes;
        }
    }
}
