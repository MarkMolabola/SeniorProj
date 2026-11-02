using MDRNJ.Shared.DTOs.Friend;

namespace MDRNJfitness.Helpers;

/// <summary>A friend's status light changed (offline / online / working out).</summary>
public record PresenceChangedMessage(PresenceDto Presence);
