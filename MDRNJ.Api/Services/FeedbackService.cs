using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Feedback;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>Bug reports and suggestions from players, and the admin inbox that reads them.</summary>
    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _db;

        public FeedbackService(AppDbContext db) => _db = db;

        public async Task SubmitAsync(Guid userId, SubmitFeedbackDto dto)
        {
            var message = dto.Message?.Trim() ?? string.Empty;
            if (message.Length < FeedbackRules.MinLength)
                throw new ArgumentException($"Tell us a bit more (at least {FeedbackRules.MinLength} characters).");
            if (message.Length > FeedbackRules.MaxLength)
                throw new ArgumentException($"Keep it under {FeedbackRules.MaxLength} characters.");
            if (!Enum.IsDefined(dto.Kind))
                throw new ArgumentException("Pick BUG or IDEA.");

            var since = DateTime.UtcNow.AddDays(-1);
            var recent = await _db.Feedback.CountAsync(f => f.UserId == userId && f.CreatedAt >= since);
            if (recent >= FeedbackRules.MaxPerDay)
                throw new InvalidOperationException("You've sent a lot of feedback today. Thanks! Try again tomorrow.");

            _db.Feedback.Add(new FeedbackEntity
            {
                UserId = userId,
                Kind = dto.Kind,
                Message = message,
                AppVersion = Clip(dto.AppVersion, 40),
                Platform = Clip(dto.Platform, 40),
                DeviceModel = Clip(dto.DeviceModel, 100),
            });
            await _db.SaveChangesAsync();
        }

        public Task<List<FeedbackDto>> GetInboxAsync(bool includeResolved) =>
            _db.Feedback
               .Where(f => includeResolved || !f.IsResolved)
               .OrderBy(f => f.IsResolved)
               .ThenByDescending(f => f.CreatedAt)
               .Take(200)
               .Select(f => new FeedbackDto
               {
                   FeedbackId = f.FeedbackId,
                   UserId = f.UserId,
                   Username = f.User.Username,
                   Kind = f.Kind,
                   Message = f.Message,
                   AppVersion = f.AppVersion,
                   Platform = f.Platform,
                   DeviceModel = f.DeviceModel,
                   CreatedAt = f.CreatedAt,
                   IsResolved = f.IsResolved,
               })
               .ToListAsync();

        public async Task SetResolvedAsync(Guid feedbackId, bool resolved)
        {
            var updated = await _db.Feedback
                .Where(f => f.FeedbackId == feedbackId)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsResolved, resolved));
            if (updated == 0)
                throw new KeyNotFoundException("That feedback no longer exists.");
        }

        private static string? Clip(string? value, int max)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value)) return null;
            return value.Length <= max ? value : value[..max];
        }
    }
}
