using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class FriendService : IFriendService
    {
        private readonly AppDbContext _db;

        public FriendService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<FriendDto> SendFriendRequestAsync(Guid requesterId, SendFriendRequestDto dto)
        {
            var addressee = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username)
                ?? throw new KeyNotFoundException($"User '{dto.Username}' not found.");

            if (requesterId == addressee.UserId)
                throw new InvalidOperationException("Cannot send friend request to yourself.");
            if (await _db.IsBlockedEitherWayAsync(requesterId, addressee.UserId))
                throw new InvalidOperationException("You can't add this player.");

            var friendship = await _db.Friendships.FirstOrDefaultAsync(f =>
                (f.RequesterId == requesterId && f.AddresseeId == addressee.UserId) ||
                (f.RequesterId == addressee.UserId && f.AddresseeId == requesterId));

            if (friendship is null)
            {
                friendship = new FriendshipEntity
                {
                    RequesterId = requesterId,
                    AddresseeId = addressee.UserId,
                    Status = FriendshipStatus.Pending
                };
                _db.Friendships.Add(friendship);
            }
            else if (friendship.Status == FriendshipStatus.Pending && friendship.AddresseeId == requesterId)
            {
                // They already asked us: treat this as accepting.
                friendship.Status = FriendshipStatus.Accepted;
            }
            else if (friendship.Status == FriendshipStatus.Declined)
            {
                // An old declined request shouldn't block a new one.
                friendship.RequesterId = requesterId;
                friendship.AddresseeId = addressee.UserId;
                friendship.Status = FriendshipStatus.Pending;
                friendship.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException(friendship.Status == FriendshipStatus.Accepted
                    ? "You are already friends."
                    : "Friend request already exists.");
            }

            await _db.SaveChangesAsync();

            return await BuildFriendDtoAsync(friendship, requesterId);
        }

        public async Task<FriendDto> AcceptFriendRequestAsync(Guid addresseeId, Guid friendshipId)
        {
            var friendship = await _db.Friendships
                .FirstOrDefaultAsync(f => f.FriendshipId == friendshipId && f.AddresseeId == addresseeId)
                ?? throw new KeyNotFoundException("Friend request not found.");

            friendship.Status = FriendshipStatus.Accepted;
            await _db.SaveChangesAsync();

            return await BuildFriendDtoAsync(friendship, addresseeId);
        }

        public async Task DeclineFriendRequestAsync(Guid addresseeId, Guid friendshipId)
        {
            var friendship = await _db.Friendships
                .FirstOrDefaultAsync(f => f.FriendshipId == friendshipId && f.AddresseeId == addresseeId)
                ?? throw new KeyNotFoundException("Friend request not found.");

            friendship.Status = FriendshipStatus.Declined;
            await _db.SaveChangesAsync();
        }

        public async Task RemoveFriendAsync(Guid userId, Guid friendUserId)
        {
            var friendship = await _db.Friendships.FirstOrDefaultAsync(f =>
                ((f.RequesterId == userId && f.AddresseeId == friendUserId) ||
                 (f.RequesterId == friendUserId && f.AddresseeId == userId)) &&
                f.Status == FriendshipStatus.Accepted)
                ?? throw new KeyNotFoundException("Friendship not found.");

            _db.Friendships.Remove(friendship);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FriendDto>> GetFriendsAsync(Guid userId)
        {
            var friendships = await _db.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .Where(f => (f.RequesterId == userId || f.AddresseeId == userId)
                            && f.Status == FriendshipStatus.Accepted)
                .ToListAsync();

            return friendships.Select(f => BuildFriendDtoFromEntity(f, userId)).ToList();
        }

        public async Task<List<FriendDto>> GetPendingRequestsAsync(Guid userId)
        {
            var friendships = await _db.Friendships
                .Include(f => f.Requester)
                .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
                .ToListAsync();

            return friendships.Select(f => BuildFriendDtoFromEntity(f, userId)).ToList();
        }

        public async Task<FriendPublicProfileDto> GetFriendProfileAsync(Guid requestingUserId, Guid friendUserId)
        {
            // Anyone's hero and stats are public (e.g. from the leaderboard); recent workouts are friends-only.
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == friendUserId)
                ?? throw new KeyNotFoundException("User not found.");

            var friendship = await _db.Friendships.FirstOrDefaultAsync(f =>
                (f.RequesterId == requestingUserId && f.AddresseeId == friendUserId) ||
                (f.RequesterId == friendUserId && f.AddresseeId == requestingUserId));

            var relationship =
                requestingUserId == friendUserId ? FriendRelationship.Self
                : friendship?.Status == FriendshipStatus.Accepted ? FriendRelationship.Friends
                : friendship?.Status == FriendshipStatus.Pending
                    ? (friendship.RequesterId == requestingUserId ? FriendRelationship.RequestSent : FriendRelationship.RequestReceived)
                : FriendRelationship.None;

            var stats = await _db.GameStats.FirstOrDefaultAsync(s => s.UserId == friendUserId);
            var activityScore = await _db.ActivityScores.FirstOrDefaultAsync(a => a.UserId == friendUserId);
            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == friendUserId);

            var showWorkouts = relationship is FriendRelationship.Friends or FriendRelationship.Self;
            var recentSessions = !showWorkouts ? new List<WorkoutSessionEntity>() : await _db.WorkoutSessions
                .Where(s => s.UserId == friendUserId && s.EndTime != null)
                .OrderByDescending(s => s.StartTime)
                .Take(3)
                .ToListAsync();

            return new FriendPublicProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                Relationship = relationship,
                PendingFriendshipId = relationship == FriendRelationship.RequestReceived ? friendship!.FriendshipId : null,
                IsBlocked = await _db.UserBlocks.AnyAsync(b => b.BlockerId == requestingUserId && b.BlockedId == friendUserId),
                CurrentLevel = stats?.CurrentLevel ?? 1,
                TotalXP = stats?.TotalXP ?? 0,
                TotalWorkouts = stats?.TotalWorkouts ?? 0,
                CurrentStreak = stats?.CurrentStreak ?? 0,
                DominantSport = activityScore?.DominantSport.ToString() ?? "None",
                SpriteState = character?.OutfitSlot ?? "default",
                AvatarId = character?.AvatarId ?? "classic",
                HeadGear = character?.HeadGear,
                TopGear = character?.TopGear,
                BottomGear = character?.BottomGear,
                ShoesGear = character?.ShoesGear,
                AccessoryGear = character?.AccessoryGear,
                RecentWorkouts = recentSessions.Select(s => new WorkoutDto
                {
                    SessionId = s.SessionId,
                    WorkoutType = s.WorkoutType.ToString(),
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DurationMins = s.DurationMins,
                    XPAwarded = s.XPAwarded,
                    Notes = s.Notes
                }).ToList()
            };
        }

        private async Task<FriendDto> BuildFriendDtoAsync(FriendshipEntity friendship, Guid currentUserId)
        {
            await _db.Entry(friendship).Reference(f => f.Requester).LoadAsync();
            await _db.Entry(friendship).Reference(f => f.Addressee).LoadAsync();
            return BuildFriendDtoFromEntity(friendship, currentUserId);
        }

        private static FriendDto BuildFriendDtoFromEntity(FriendshipEntity f, Guid currentUserId)
        {
            var friend = f.RequesterId == currentUserId ? f.Addressee : f.Requester;
            return new FriendDto
            {
                FriendshipId = f.FriendshipId,
                UserId = friend.UserId,
                Username = friend.Username,
                AvatarUrl = friend.AvatarUrl,
                Status = f.Status.ToString(),
                CreatedAt = f.CreatedAt
            };
        }
    }
}
