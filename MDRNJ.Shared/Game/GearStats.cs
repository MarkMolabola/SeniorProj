namespace MDRNJ.Shared.Game
{
    /// <summary>Shop gear rarity. Shown as the item's outline: grey, blue, purple, gold.</summary>
    public enum GearRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    /// <summary>The three hero stats, as boosted by gear.</summary>
    public enum StatKind
    {
        Strength = 0,   // STR
        Endurance = 1,  // END (cardio)
        Flexibility = 2 // FLX
    }

    /// <summary>
    /// Stat boosts from worn gear. Boosts are stat points (StatProgression: 10 points
    /// = 1 level) and count everywhere trained points do: Hero tab levels,
    /// stat-gated achievements and dungeon power.
    /// </summary>
    public static class GearStats
    {
        public static int BonusPoints(GearRarity rarity) => rarity switch
        {
            GearRarity.Common => 2,
            GearRarity.Rare => 5,
            GearRarity.Epic => 10,
            _ => 20
        };

        public static string ShortName(StatKind stat) => stat switch
        {
            StatKind.Strength => "STR",
            StatKind.Endurance => "END",
            _ => "FLX"
        };

        /// <summary>
        /// Total boost per stat for the selected hero: a special hero's built-in outfit,
        /// or else the gear the Classic Hero is wearing.
        /// </summary>
        public static (float Strength, float Endurance, float Flexibility) Bonus(IEquippedGear? gear, string? avatarId)
        {
            if (HeroAvatars.Find(avatarId) is not { IsSpecial: true } hero)
                return Bonus(gear);

            float str = 0, end = 0, flx = 0;
            foreach (var piece in hero.Pieces)
            {
                switch (piece.Stat)
                {
                    case StatKind.Strength: str += piece.Bonus; break;
                    case StatKind.Endurance: end += piece.Bonus; break;
                    default: flx += piece.Bonus; break;
                }
            }
            return (str, end, flx);
        }

        /// <summary>Total boost per stat from everything the hero is wearing.</summary>
        public static (float Strength, float Endurance, float Flexibility) Bonus(IEquippedGear? gear)
        {
            float str = 0, end = 0, flx = 0;
            if (gear is null) return (str, end, flx);

            foreach (var slot in Enum.GetValues<GearSlot>())
            {
                if (GearCatalog.Find(gear.Get(slot)) is not { } item) continue;
                switch (item.Stat)
                {
                    case StatKind.Strength: str += item.Bonus; break;
                    case StatKind.Endurance: end += item.Bonus; break;
                    default: flx += item.Bonus; break;
                }
            }
            return (str, end, flx);
        }
    }
}
