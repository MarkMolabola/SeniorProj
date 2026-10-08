using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Challenge;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _character;

        public ChallengeService(AppDbContext db, IMapper mapper, ICharacterService character)
        {
            _db = db;
            _mapper = mapper;
            _character = character;
        }

        public async Task<ChallengeDto> CreateAsync(Guid userId, CreateChallengeDto dto)
        {
            if (!Enum.TryParse<ChallengeType>(dto.ChallengeType, true, out var challengeType))
                throw new ArgumentException($"Unknown challenge type: {dto.ChallengeType}");

            var challenge = new ChallengeEntity
            {
                CreatorId = userId,
                Title = dto.Title,
                Description = dto.Description,
                ChallengeType = challengeType,
                TargetValue = dto.TargetValue,
                XPReward = dto.XPReward,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = ChallengeStatus.Active
            };

            _db.Challenges.Add(challenge);

            _db.ChallengeParticipants.Add(new ChallengeParticipantEntity
            {
                ChallengeId = challenge.ChallengeId,
                UserId = userId,
                Status = ChallengeStatus.Active
            });

            await _db.SaveChangesAsync();

            return await GetChallengeDto(challenge.ChallengeId, userId);
        }

        public async Task<ChallengeDto> JoinAsync(Guid userId, Guid challengeId)
        {
            var challenge = await _db.Challenges.FirstOrDefaultAsync(c => c.ChallengeId == challengeId)
                ?? throw new KeyNotFoundException("Challenge not found.");

            var alreadyJoined = await _db.ChallengeParticipants
                .AnyAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);

            if (alreadyJoined)
                throw new InvalidOperationException("Already joined this challenge.");

            _db.ChallengeParticipants.Add(new ChallengeParticipantEntity
            {
                ChallengeId = challengeId,
                UserId = userId,
                Status = ChallengeStatus.Active
            });

            await _db.SaveChangesAsync();
            return await GetChallengeDto(challengeId, userId);
        }

        public async Task<List<ChallengeDto>> GetActiveAsync(Guid userId)
        {
            var joinedIds = await _db.ChallengeParticipants
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.ChallengeId)
                .ToListAsync();

            var challenges = await _db.Challenges
                .Where(c => c.Status == ChallengeStatus.Active && !joinedIds.Contains(c.ChallengeId))
                .Select(c => c.ChallengeId)
                .ToListAsync();

            var result = new List<ChallengeDto>();
            foreach (var id in challenges)
                result.Add(await GetChallengeDto(id, userId));

            return result;
        }

        public async Task<List<ChallengeDto>> GetUserChallengesAsync(Guid userId)
        {
            var challengeIds = await _db.ChallengeParticipants
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.ChallengeId)
                .ToListAsync();

            var result = new List<ChallengeDto>();
            foreach (var id in challengeIds)
                result.Add(await GetChallengeDto(id, userId));

            return result;
        }

        public async Task<ChallengeDto> UpdateProgressAsync(Guid userId, Guid challengeId, float progress)
        {
            var participant = await _db.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId)
                ?? throw new KeyNotFoundException("Challenge participation not found.");

            var challenge = await _db.Challenges.FirstOrDefaultAsync(c => c.ChallengeId == challengeId)
                ?? throw new KeyNotFoundException("Challenge not found.");

            participant.Progress = progress;
            var completedNow = false;
            if (progress >= challenge.TargetValue && participant.Status != ChallengeStatus.Completed)
            {
                participant.Status = ChallengeStatus.Completed;
                completedNow = true;
            }

            await _db.SaveChangesAsync();

            var dto = await GetChallengeDto(challengeId, userId);
            if (completedNow)
                dto.CharacterProgress = await _character.ApplyActivityAsync(
                    userId, StatRules.ForCompletion(challenge.ChallengeType, challenge.XPReward));
            return dto;
        }

        private async Task<ChallengeDto> GetChallengeDto(Guid challengeId, Guid userId)
        {
            var challenge = await _db.Challenges
                .Include(c => c.Participants)
                    .ThenInclude(cp => cp.User)
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId)
                ?? throw new KeyNotFoundException("Challenge not found.");

            var userParticipant = challenge.Participants.FirstOrDefault(cp => cp.UserId == userId);

            return new ChallengeDto
            {
                ChallengeId = challenge.ChallengeId,
                CreatorId = challenge.CreatorId,
                Title = challenge.Title,
                Description = challenge.Description,
                ChallengeType = challenge.ChallengeType.ToString(),
                TargetValue = challenge.TargetValue,
                XPReward = challenge.XPReward,
                StartDate = challenge.StartDate,
                EndDate = challenge.EndDate,
                Status = challenge.Status.ToString(),
                Progress = userParticipant?.Progress ?? 0,
                Participants = challenge.Participants.Select(cp => new MDRNJ.Shared.DTOs.Challenge.ChallengeParticipantDto
                {
                    UserId = cp.UserId,
                    Username = cp.User.Username,
                    Progress = cp.Progress,
                    IsCompleted = cp.Status == ChallengeStatus.Completed
                }).ToList()
            };
        }
    }
}
