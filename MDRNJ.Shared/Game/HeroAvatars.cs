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

        public static readonly IReadOnlyList<(string Id, string Name)> All = new[]
        {
            (Classic, "Classic Hero"),
            (StartingGirl, "Starter Girl"),
        };

        public static bool IsValid(string? id) => All.Any(a => a.Id == id);
    }
}
