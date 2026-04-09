using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Quest;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace MDRNJ.Api.Services
{
    public class QuestService : IQuestService
    {
        private readonly AppDbContext _db;

        public QuestService(AppDbContext db) => _db = db;

        public async Task<QuestDto> CreateAsync(CreateQuestDto dto)
        {
            if (!Enum.TryParse<ChallengeType>(dto.QuestType, true, out var questType))
                throw new ArgumentException($"Unknown quest type: {dto.QuestType}");

            var quest = new QuestEntity
            {
                Title = dto.Title,
                Description = dto.Description,
                XPReward = dto.XPReward,
                QuestType = questType,
                TargetValue = dto.TargetValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _db.Quests.Add(quest);
            await _db.SaveChangesAsync();

            return ToDto(quest, 0);
        }

        public async Task<QuestDto> UpdateAsync(Guid questId, UpdateQuestDto dto)
        {
            var quest = await _db.Quests.Include(q => q.UserQuests)
                .FirstOrDefaultAsync(q => q.QuestId == questId)
                ?? throw new KeyNotFoundException("Quest not found.");

            if (dto.Title is not null) quest.Title = dto.Title;
            if (dto.Description is not null) quest.Description = dto.Description;
            if (dto.XPReward.HasValue) quest.XPReward = dto.XPReward.Value;
            if (dto.TargetValue.HasValue) quest.TargetValue = dto.TargetValue.Value;
            if (dto.StartDate.HasValue) quest.StartDate = dto.StartDate;
            if (dto.EndDate.HasValue) quest.EndDate = dto.EndDate;
            if (dto.IsActive.HasValue) quest.IsActive = dto.IsActive.Value;
            if (dto.QuestType is not null)
            {
                if (!Enum.TryParse<ChallengeType>(dto.QuestType, true, out var questType))
                    throw new ArgumentException($"Unknown quest type: {dto.QuestType}");
                quest.QuestType = questType;
            }

            await _db.SaveChangesAsync();
            return ToDto(quest, quest.UserQuests.Count);
        }

        public async Task DeleteAsync(Guid questId)
        {
            var quest = await _db.Quests.FirstOrDefaultAsync(q => q.QuestId == questId)
                ?? throw new KeyNotFoundException("Quest not found.");

            _db.Quests.Remove(quest);
            await _db.SaveChangesAsync();
        }

        public async Task<List<QuestDto>> GetAllAsync()
        {
            var quests = await _db.Quests
                .Include(q => q.UserQuests)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return quests.Select(q => ToDto(q, q.UserQuests.Count)).ToList();
        }

        public async Task SendToAllUsersAsync(Guid questId)
        {
            var quest = await _db.Quests.FirstOrDefaultAsync(q => q.QuestId == questId)
                ?? throw new KeyNotFoundException("Quest not found.");

            var allUserIds = await _db.Users.Select(u => u.UserId).ToListAsync();
            var alreadyAssigned = await _db.UserQuests
                .Where(uq => uq.QuestId == questId)
                .Select(uq => uq.UserId)
                .ToListAsync();

            var toAssign = allUserIds.Except(alreadyAssigned).ToList();
            foreach (var userId in toAssign)
            {
                _db.UserQuests.Add(new UserQuestEntity
                {
                    UserId = userId,
                    QuestId = questId
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task SendToUserAsync(Guid questId, Guid userId)
        {
            var quest = await _db.Quests.FirstOrDefaultAsync(q => q.QuestId == questId)
                ?? throw new KeyNotFoundException("Quest not found.");

            var exists = await _db.UserQuests
                .AnyAsync(uq => uq.QuestId == questId && uq.UserId == userId);

            if (exists) return;

            _db.UserQuests.Add(new UserQuestEntity
            {
                UserId = userId,
                QuestId = questId
            });

            await _db.SaveChangesAsync();
        }

        public async Task<List<UserQuestDto>> GetMyQuestsAsync(Guid userId)
        {
            var userQuests = await _db.UserQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId)
                .OrderByDescending(uq => uq.AssignedAt)
                .ToListAsync();

            return userQuests.Select(ToUserQuestDto).ToList();
        }

        public async Task<UserQuestDto> UpdateProgressAsync(Guid userId, Guid questId, float progress)
        {
            var userQuest = await _db.UserQuests
                .Include(uq => uq.Quest)
                .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId)
                ?? throw new KeyNotFoundException("Quest assignment not found.");

            userQuest.Progress = progress;
            if (progress >= userQuest.Quest.TargetValue && userQuest.Status == QuestStatus.Active)
            {
                userQuest.Status = QuestStatus.Completed;
                userQuest.CompletedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return ToUserQuestDto(userQuest);
        }

        private static QuestDto ToDto(QuestEntity q, int assignedCount) => new()
        {
            QuestId = q.QuestId,
            Title = q.Title,
            Description = q.Description,
            XPReward = q.XPReward,
            QuestType = q.QuestType.ToString(),
            TargetValue = q.TargetValue,
            StartDate = q.StartDate,
            EndDate = q.EndDate,
            IsActive = q.IsActive,
            CreatedAt = q.CreatedAt,
            AssignedCount = assignedCount
        };

        private static UserQuestDto ToUserQuestDto(UserQuestEntity uq) => new()
        {
            QuestId = uq.Quest.QuestId,
            Title = uq.Quest.Title,
            Description = uq.Quest.Description,
            XPReward = uq.Quest.XPReward,
            QuestType = uq.Quest.QuestType.ToString(),
            TargetValue = uq.Quest.TargetValue,
            Progress = uq.Progress,
            Status = uq.Status.ToString(),
            StartDate = uq.Quest.StartDate,
            EndDate = uq.Quest.EndDate,
            AssignedAt = uq.AssignedAt,
            CompletedAt = uq.CompletedAt
        };
    }
}
