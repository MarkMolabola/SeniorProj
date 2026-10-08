namespace MDRNJ.Shared.Game
{
    /// <summary>
    /// Hero looks a player can pick on the Hero tab. Shared so the API only
    /// accepts known ids and the app can list them. To add one, add its
    /// sprite in the app (HeroSprites) and an entry here.
    /// </summary>
    public static class HeroAvatars
    {
        /// <summary>The layered hero whose outfit changes with training.</summary>
        public const string Classic = "classic";
        public const string StartingGirl = "starting_girl";
        public const string Archer = "starter_archer";
        public const string Archer2 = "starter_archer2";
        public const string BuffNinja = "starting_buff_ninja";
        public const string StartingGirl2 = "starting_girl2";
        public const string Ninja = "starting_ninja";

        public static readonly IReadOnlyList<(string Id, string Name)> All = new[]
        {
            (Classic, "Classic Hero"),
            (StartingGirl, "Starter Girl"),
            (StartingGirl2, "Starter Girl II"),
            (Archer, "Archer"),
            (Archer2, "Archer II"),
            (Ninja, "Ninja"),
            (BuffNinja, "Buff Ninja"),
        };

        public static bool IsValid(string? id) => All.Any(a => a.Id == id);
    }
}
