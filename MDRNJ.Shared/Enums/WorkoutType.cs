namespace MDRNJ.Shared.Enums
{
    /// <summary>
    /// Stored as its number, so existing values must never change: add new
    /// types at the end. <see cref="MDRNJ.Shared.Game.WorkoutTypes.Categories"/>
    /// groups them for the picker.
    /// </summary>
    public enum WorkoutType
    {
        Weightlifting = 0,
        Running = 1,
        Cycling = 2,
        Swimming = 3,
        Tennis = 4,
        Boxing = 5,
        Yoga = 6,
        HIIT = 7,
        Basketball = 8,
        Football = 9,
        Golf = 10,
        Hiking = 11,
        Rowing = 12,
        Climbing = 13,

        // Cardio
        Walking = 14,
        Jogging = 15,
        JumpRope = 16,
        Elliptical = 17,
        StairClimber = 18,
        Dance = 19,
        OtherCardio = 20,

        // Flexibility
        Stretching = 21,
        Pilates = 22,
        Mobility = 23,
        TaiChi = 24,
        OtherFlexibility = 25,

        // Strength
        Calisthenics = 26,
        Powerlifting = 27,
        CircuitTraining = 28,
        Kettlebell = 29,
        OtherStrength = 30,

        // Sports
        Soccer = 31,
        Volleyball = 32,
        Baseball = 33,
        Badminton = 34,
        Pickleball = 35,
        MartialArts = 36,
        OtherSport = 37,

        /// <summary>Legacy catch-all from before categories; no longer offered.</summary>
        Other = 99
    }
}
