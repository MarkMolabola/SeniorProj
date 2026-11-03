namespace MDRNJ.Shared.Enums
{
    /// <summary>The status light next to a friend: red / yellow / green.</summary>
    public enum PresenceStatus
    {
        Offline = 0,
        /// <summary>Has the app open.</summary>
        Online = 1,
        /// <summary>Has a workout timer running.</summary>
        WorkingOut = 2
    }
}
