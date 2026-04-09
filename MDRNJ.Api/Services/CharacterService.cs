using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly CharacterClassifierService _classifier;

        public CharacterService(AppDbContext db, IMapper mapper, CharacterClassifierService classifier)
        {
            _db = db;
            _mapper = mapper;
            _classifier = classifier;
        }

        public async Task<CharacterStateDto> GetCharacterAsync(Guid userId)
        {
            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new KeyNotFoundException("Character not found.");

            return _mapper.Map<CharacterStateDto>(character);
        }

        public async Task<CharacterStateDto> EvolveCharacterAsync(Guid userId)
        {
            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new KeyNotFoundException("Character not found.");

            await RecalculateActivityScoreAsync(userId);
            var activityScore = await _db.ActivityScores.FirstOrDefaultAsync(a => a.UserId == userId);
            if (activityScore is not null)
            {
                character.BodyType = await _classifier.ClassifyBodyTypeAsync(activityScore);

                character.OutfitSlot = activityScore.DominantSport switch
                {
                    SportType.Strength    => "strength",
                    SportType.Cardio      => "cardio",
                    SportType.Flexibility => "flexibility",
                    _                     => "default"
                };

                character.EquipmentSlot = activityScore.DominantSport switch
                {
                    SportType.Strength    => "dumbbells",
                    SportType.Cardio      => "sneakers",
                    SportType.Flexibility => "mat",
                    _                     => "default"
                };
            }

            character.LastEvolved = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return _mapper.Map<CharacterStateDto>(character);
        }

        public async Task<ActivityScoreDto> GetActivityScoreAsync(Guid userId)
        {
            var score = await _db.ActivityScores.FirstOrDefaultAsync(a => a.UserId == userId)
                ?? throw new KeyNotFoundException("Activity score not found.");

            return _mapper.Map<ActivityScoreDto>(score);
        }

        public async Task<ActivityScoreDto> RecalculateActivityScoreAsync(Guid userId)
        {
            var sessions = await _db.WorkoutSessions
                .Include(s => s.Exercises)
                .Where(s => s.UserId == userId && s.EndTime != null)
                .ToListAsync();

            var score = await _db.ActivityScores.FirstOrDefaultAsync(a => a.UserId == userId)
                ?? new ActivityScoreEntity { UserId = userId };

            float strength = 0, cardio = 0, flexibility = 0;
            foreach (var session in sessions)
            {
                switch (session.WorkoutType)
                {
                    case WorkoutType.Weightlifting:
                        strength += session.DurationMins; break;
                    case WorkoutType.Running:
                    case WorkoutType.Cycling:
                    case WorkoutType.Swimming:
                    case WorkoutType.Rowing:
                        cardio += session.DurationMins; break;
                    case WorkoutType.Yoga:
                        flexibility += session.DurationMins; break;
                    default:
                        cardio += session.DurationMins * 0.5f;
                        strength += session.DurationMins * 0.3f;
                        flexibility += session.DurationMins * 0.2f;
                        break;
                }
            }

            float total = strength + cardio + flexibility;
            if (total > 0)
            {
                score.StrengthScore = strength / total * 100;
                score.CardioScore = cardio / total * 100;
                score.FlexibilityScore = flexibility / total * 100;
            }

            score.DominantSport = total == 0
                ? SportType.Cardio
                : strength > cardio && strength > flexibility
                    ? SportType.Strength
                    : cardio >= flexibility
                        ? SportType.Cardio
                        : SportType.Flexibility;

            score.LastCalculated = DateTime.UtcNow;

            if (score.ScoreId == Guid.Empty)
                _db.ActivityScores.Add(score);

            await _db.SaveChangesAsync();

            return _mapper.Map<ActivityScoreDto>(score);
        }
    }
}
