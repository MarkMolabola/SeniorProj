using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.User;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IChatService _chat;

        public UserService(AppDbContext db, IMapper mapper, IChatService chat)
        {
            _db = db;
            _mapper = mapper;
            _chat = chat;
        }

        public async Task<UserDto> GetByIdAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new KeyNotFoundException("User not found.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
                ?? throw new KeyNotFoundException("Profile not found.");
            return _mapper.Map<UserProfileDto>(profile);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto)
        {
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
                ?? throw new KeyNotFoundException("Profile not found.");

            profile.DisplayName = dto.DisplayName;
            profile.Bio = dto.Bio;
            profile.WeightKg = dto.WeightKg;
            profile.HeightCm = dto.HeightCm;

            if (Enum.TryParse<FitnessGoal>(dto.FitnessGoal, out var goal))
                profile.FitnessGoal = goal;

            await _db.SaveChangesAsync();
            return _mapper.Map<UserProfileDto>(profile);
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new KeyNotFoundException("User not found.");

            // Groups first, so ownership passes on (and empty groups go) as if they'd left
            var groupIds = await _db.ChatMembers
                .Where(m => m.UserId == userId && m.Channel.Kind == ChatChannelKind.Group)
                .Select(m => m.ChannelId)
                .ToListAsync();
            foreach (var groupId in groupIds)
                await _chat.LeaveGroupAsync(userId, groupId);

            // Rows that point at the user without cascading (two links to Users each).
            // Each step is safe to repeat, so a failed deletion can just be retried.
            await _db.ChatReports.Where(r => r.ReporterId == userId).ExecuteDeleteAsync();
            await _db.UserBlocks.Where(b => b.BlockerId == userId || b.BlockedId == userId).ExecuteDeleteAsync();
            await _db.Friendships.Where(f => f.RequesterId == userId || f.AddresseeId == userId).ExecuteDeleteAsync();
            await _db.Challenges.Where(c => c.ChallengerId == userId || c.OpponentId == userId).ExecuteDeleteAsync();

            // Chats this user posted in need new list previews once their messages are gone
            var postedIn = await _db.ChatMessages
                .Where(m => m.SenderId == userId)
                .Select(m => m.ChannelId)
                .Distinct()
                .ToListAsync();

            // Everything else (stats, workouts, gear, messages, tokens...) cascades with the user
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            foreach (var channelId in postedIn)
                await _chat.RefreshPreviewAsync(channelId);
        }
    }
}
