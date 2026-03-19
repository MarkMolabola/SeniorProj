using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.User;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public UserService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
    }
}
