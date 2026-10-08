using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class CharacterService : ICharacterService
    {
        /// <summary>Training shares (and so body type / outfit) use this many recent days.</summary>
        private const int ShareWindowDays = 30;

        /// <summary>Recent training minutes needed before the character can evolve.</summary>
        private const float MinMinutesToEvolve = 60;

        /// <summary>
        /// A new outfit needs this many share points over the current one, so a
        /// single off-style session doesn't flip the outfit back and forth.
        /// </summary>
        private const float OutfitSwitchMargin = 10;

        /// <summary>Body type changes at most this often, unless the outfit changes too.</summary>
        private static readonly TimeSpan BodyTypeCooldown = TimeSpan.FromDays(1);

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

        public async Task<CharacterStateDto> SetAvatarAsync(Guid userId, string avatarId)
        {
            if (!HeroAvatars.IsValid(avatarId))
                throw new ArgumentException($"Unknown avatar: {avatarId}");

            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new KeyNotFoundException("Character not found.");

            character.AvatarId = avatarId;
            await _db.SaveChangesAsync();
            return _mapper.Map<CharacterStateDto>(character);
        }

        public async Task<CharacterProgressDto> ApplyActivityAsync(Guid userId, StatGain gain)
        {
            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new KeyNotFoundException("Character not found.");
            var score = await GetOrCreateScoreAsync(userId);

            // Shares first, so a bonus is split by up-to-date training habits
            var recentMinutes = await UpdateSharesAsync(score, userId);
            var (strength, cardio, flexibility) = SplitBonus(gain, score);

            var levelUps = new List<StatLevelUpDto>();
            score.StrengthPoints = AddPoints(score.StrengthPoints, strength, "STR", levelUps);
            score.CardioPoints = AddPoints(score.CardioPoints, cardio, "END", levelUps);
            score.FlexibilityPoints = AddPoints(score.FlexibilityPoints, flexibility, "FLX", levelUps);

            var previousOutfit = character.OutfitSlot;
            var previousBodyType = character.BodyType;
            var evolved = await TryEvolveAsync(character, score, recentMinutes);

            await _db.SaveChangesAsync();

            return new CharacterProgressDto
            {
                StrengthGained = strength,
                CardioGained = cardio,
                FlexibilityGained = flexibility,
                LevelUps = levelUps,
                Evolved = evolved,
                PreviousOutfitSlot = evolved ? previousOutfit : null,
                PreviousBodyType = evolved ? (int)previousBodyType : null,
                Character = _mapper.Map<CharacterStateDto>(character),
                ActivityScore = _mapper.Map<ActivityScoreDto>(score)
            };
        }

        public async Task<ActivityScoreDto> GetActivityScoreAsync(Guid userId)
        {
            var score = await _db.ActivityScores.FirstOrDefaultAsync(a => a.UserId == userId)
                ?? throw new KeyNotFoundException("Activity score not found.");

            return _mapper.Map<ActivityScoreDto>(score);
        }

        public async Task<ActivityScoreDto> RecalculateActivityScoreAsync(Guid userId)
        {
            var score = await GetOrCreateScoreAsync(userId);
            await UpdateSharesAsync(score, userId);
            await _db.SaveChangesAsync();

            return _mapper.Map<ActivityScoreDto>(score);
        }

        private async Task<ActivityScoreEntity> GetOrCreateScoreAsync(Guid userId)
        {
            var score = await _db.ActivityScores.FirstOrDefaultAsync(a => a.UserId == userId);
            if (score is null)
            {
                score = new ActivityScoreEntity { UserId = userId };
                _db.ActivityScores.Add(score);
            }
            return score;
        }

        /// <summary>
        /// Recomputes the recent training shares (sum to 100) and dominant sport.
        /// With no recent training the last known shares are kept, so the
        /// character doesn't reset after a break. Returns minutes trained in the window.
        /// </summary>
        private async Task<float> UpdateSharesAsync(ActivityScoreEntity score, Guid userId)
        {
            var since = DateTime.UtcNow.AddDays(-ShareWindowDays);
            var sessions = await _db.WorkoutSessions
                .Where(s => s.UserId == userId && s.EndTime != null && s.StartTime >= since)
                .Select(s => new { s.WorkoutType, s.DurationMins })
                .ToListAsync();

            float strength = 0, cardio = 0, flexibility = 0;
            foreach (var session in sessions)
            {
                var (s, c, f) = StatRules.Weights(session.WorkoutType);
                strength += s * session.DurationMins;
                cardio += c * session.DurationMins;
                flexibility += f * session.DurationMins;
            }

            float total = strength + cardio + flexibility;
            if (total > 0)
            {
                score.StrengthScore = strength / total * 100;
                score.CardioScore = cardio / total * 100;
                score.FlexibilityScore = flexibility / total * 100;
                score.DominantSport = strength > cardio && strength > flexibility
                    ? SportType.Strength
                    : cardio >= flexibility
                        ? SportType.Cardio
                        : SportType.Flexibility;
            }

            score.LastCalculated = DateTime.UtcNow;
            return total;
        }

        private static (float Strength, float Cardio, float Flexibility) SplitBonus(StatGain gain, ActivityScoreEntity score)
        {
            float strength = gain.Strength, cardio = gain.Cardio, flexibility = gain.Flexibility;
            if (gain.Bonus <= 0)
                return (strength, cardio, flexibility);

            var total = score.StrengthScore + score.CardioScore + score.FlexibilityScore;
            if (total <= 0)
                return (strength + gain.Bonus / 3, cardio + gain.Bonus / 3, flexibility + gain.Bonus / 3);

            return (
                strength + gain.Bonus * score.StrengthScore / total,
                cardio + gain.Bonus * score.CardioScore / total,
                flexibility + gain.Bonus * score.FlexibilityScore / total);
        }

        private static float AddPoints(float current, float gained, string stat, List<StatLevelUpDto> levelUps)
        {
            var updated = current + Math.Max(0, gained);
            var newLevel = StatProgression.LevelFor(updated);
            if (newLevel > StatProgression.LevelFor(current))
                levelUps.Add(new StatLevelUpDto { Stat = stat, NewLevel = newLevel });
            return updated;
        }

        /// <summary>Updates outfit / body type from the training shares. True if anything changed.</summary>
        private async Task<bool> TryEvolveAsync(CharacterStateEntity character, ActivityScoreEntity score, float recentMinutes)
        {
            if (recentMinutes < MinMinutesToEvolve)
                return false;

            var outfit = OutfitFor(score.DominantSport);
            var currentShare = ShareFor(character.OutfitSlot, score);
            var outfitChanges = outfit != character.OutfitSlot
                && (currentShare is null || ShareFor(outfit, score) - currentShare >= OutfitSwitchMargin);

            var bodyType = await _classifier.ClassifyBodyTypeAsync(score);
            var bodyTypeChanges = bodyType != character.BodyType
                && (outfitChanges || DateTime.UtcNow - character.LastEvolved >= BodyTypeCooldown);

            if (!outfitChanges && !bodyTypeChanges)
                return false;

            if (outfitChanges)
            {
                character.OutfitSlot = outfit;
                character.EquipmentSlot = EquipmentFor(score.DominantSport);
            }
            if (bodyTypeChanges)
                character.BodyType = bodyType;

            character.LastEvolved = DateTime.UtcNow;
            return true;
        }

        private static string OutfitFor(SportType sport) => sport switch
        {
            SportType.Strength    => "strength",
            SportType.Cardio      => "cardio",
            SportType.Flexibility => "flexibility",
            _                     => "default"
        };

        private static string EquipmentFor(SportType sport) => sport switch
        {
            SportType.Strength    => "dumbbells",
            SportType.Cardio      => "sneakers",
            SportType.Flexibility => "mat",
            _                     => "default"
        };

        /// <summary>The training share behind an outfit, or null for "default" / unknown outfits.</summary>
        private static float? ShareFor(string outfitSlot, ActivityScoreEntity score) => outfitSlot switch
        {
            "strength"    => score.StrengthScore,
            "cardio"      => score.CardioScore,
            "flexibility" => score.FlexibilityScore,
            _             => null
        };
    }
}
