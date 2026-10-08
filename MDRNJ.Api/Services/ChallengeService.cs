using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Challenge;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class ChallengeService : IChallengeService
    {
        public const string DuelKind = "Duel";

        private readonly AppDbContext _db;
        private readonly ICharacterService _character;
        private readonly IGoalProgressService _goals;
        private readonly IAchievementService _achievements;

        public ChallengeService(AppDbContext db, ICharacterService character,
            IGoalProgressService goals, IAchievementService achievements)
        {
            _db = db;
            _character = character;
            _goals = goals;
            _achievements = achievements;
        }

        public async Task<ChallengeDto> CreateAsync(Guid userId, CreateChallengeDto dto)
        {
            if (!TryParseEnum<DuelContest>(dto.Contest, out var contest))
                throw new ArgumentException($"Unknown contest: {dto.Contest}");
            if (!TryParseEnum<DuelScoring>(dto.Scoring, out var scoring))
                throw new ArgumentException($"Unknown scoring: {dto.Scoring}");
            if (!DuelRules.DurationHours.Contains(dto.DurationHours))
                throw new ArgumentException("Pick a challenge length of 1, 3 or 7 days.");
            if (dto.OpponentId == userId)
                throw new ArgumentException("You can't challenge yourself.");

            var isTimedRace = DuelRules.IsTimedRace(contest, scoring);
            if (isTimedRace && dto.DistanceKm is not > 0)
                throw new ArgumentException("Pick a race distance.");

            var areFriends = await _db.Friendships.AnyAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == userId && f.AddresseeId == dto.OpponentId) ||
                 (f.RequesterId == dto.OpponentId && f.AddresseeId == userId)));
            if (!areFriends)
                throw new InvalidOperationException("You can only challenge friends.");

            var challenge = new ChallengeEntity
            {
                ChallengerId = userId,
                OpponentId = dto.OpponentId,
                Contest = contest,
                Scoring = scoring,
                DistanceKm = isTimedRace ? dto.DistanceKm : null,
                Deadline = DateTime.UtcNow.AddHours(dto.DurationHours)
            };

            _db.Challenges.Add(challenge);
            await _db.SaveChangesAsync();
            return ToDto(await LoadAsync(userId, challenge.ChallengeId), userId);
        }

        public async Task<List<ChallengeDto>> GetMyAsync(Guid userId)
        {
            await ResolveDueAsync(userId);

            var challenges = await Query()
                .Where(c => c.ChallengerId == userId || c.OpponentId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .ToListAsync();

            return challenges.Select(c => ToDto(c, userId)).ToList();
        }

        public async Task<ChallengeDto> AcceptAsync(Guid userId, Guid challengeId)
        {
            var challenge = await LoadOpenAsync(userId, challengeId);
            if (challenge.OpponentId != userId || challenge.Status != ChallengeStatus.Pending)
                throw new InvalidOperationException("This challenge can't be accepted.");

            challenge.Status = ChallengeStatus.Active;
            await _db.SaveChangesAsync();
            return ToDto(challenge, userId);
        }

        public async Task<ChallengeDto> DeclineAsync(Guid userId, Guid challengeId)
        {
            var challenge = await LoadOpenAsync(userId, challengeId);
            if (challenge.OpponentId != userId || challenge.Status != ChallengeStatus.Pending)
                throw new InvalidOperationException("This challenge can't be declined.");

            challenge.Status = ChallengeStatus.Declined;
            challenge.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(challenge, userId);
        }

        public async Task<ChallengeDto> CancelAsync(Guid userId, Guid challengeId)
        {
            var challenge = await LoadOpenAsync(userId, challengeId);
            if (challenge.ChallengerId != userId || challenge.Status != ChallengeStatus.Pending)
                throw new InvalidOperationException("Only a challenge that hasn't been accepted can be cancelled.");

            challenge.Status = ChallengeStatus.Cancelled;
            challenge.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(challenge, userId);
        }

        public async Task<ChallengeDto> SubmitAttemptAsync(Guid userId, Guid challengeId, float value)
        {
            if (value <= 0)
                throw new ArgumentException("Enter a result greater than zero.");

            var challenge = await LoadOpenAsync(userId, challengeId);
            if (challenge.Status != ChallengeStatus.Active)
                throw new InvalidOperationException("This challenge isn't running.");

            var isChallenger = challenge.ChallengerId == userId;
            if (challenge.Scoring == DuelScoring.BestAttempt
                && challenge.Attempts.Any(a => a.UserId == userId))
                throw new InvalidOperationException("You already locked in your attempt.");

            // Added via the DbSet: the attempt's preset key would otherwise make EF treat it as existing
            _db.ChallengeAttempts.Add(new ChallengeAttemptEntity { ChallengeId = challengeId, UserId = userId, Value = value });
            if (isChallenger)
                challenge.ChallengerScore = DuelRules.Combine(challenge.ChallengerScore, value, challenge.Contest, challenge.Scoring);
            else
                challenge.OpponentScore = DuelRules.Combine(challenge.OpponentScore, value, challenge.Contest, challenge.Scoring);

            // Best attempt: done as soon as both have locked in. Total runs to the deadline.
            var bothIn = challenge.ChallengerScore is not null && challenge.OpponentScore is not null;
            if (challenge.Scoring == DuelScoring.Total || !bothIn)
            {
                await _db.SaveChangesAsync();
                return ToDto(challenge, userId);
            }

            var stats = await _goals.GetOrCreateStatsAsync(userId);
            var levelBefore = stats.CurrentLevel;
            var goldBefore = stats.Gold;

            await ResolveAndSaveAsync(new[] { challenge });

            // The submitter sees the result right away in the response
            if (isChallenger) challenge.ChallengerSeenResult = true;
            else challenge.OpponentSeenResult = true;
            await _db.SaveChangesAsync();

            var dto = ToDto(challenge, userId);
            dto.CharacterProgress = await _character.ApplyActivityAsync(
                userId, StatRules.ForDuel(challenge.Contest, dto.XPEarned));
            dto.AccountProgress = new AccountProgressDto
            {
                XPGained = dto.XPEarned,
                GoldGained = stats.Gold - goldBefore,
                PreviousLevel = levelBefore,
                NewLevel = stats.CurrentLevel,
                CompletedGoals = { ToGoal(challenge, dto) }
            };
            await ApplyOpponentStatsAsync(challenge, exceptUserId: userId);
            return dto;
        }

        public async Task MarkResultSeenAsync(Guid userId, Guid challengeId)
        {
            var challenge = await _db.Challenges.FirstOrDefaultAsync(c => c.ChallengeId == challengeId
                    && (c.ChallengerId == userId || c.OpponentId == userId))
                ?? throw new KeyNotFoundException("Challenge not found.");

            if (challenge.ChallengerId == userId) challenge.ChallengerSeenResult = true;
            else challenge.OpponentSeenResult = true;
            await _db.SaveChangesAsync();
        }

        // ── Resolution ──────────────────────────────────────────────────────

        /// <summary>Expires unanswered challenges and resolves running ones past their deadline.</summary>
        private async Task ResolveDueAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var due = await Query()
                .Where(c => (c.ChallengerId == userId || c.OpponentId == userId)
                         && (c.Status == ChallengeStatus.Pending || c.Status == ChallengeStatus.Active)
                         && c.Deadline <= now)
                .ToListAsync();
            if (due.Count == 0) return;

            await ResolveAndSaveAsync(due);
            foreach (var challenge in due)
                await ApplyOpponentStatsAsync(challenge, exceptUserId: null);
        }

        /// <summary>
        /// Settles challenges: winner, XP for both sides, achievements. Unanswered
        /// challenges and ones nobody submitted to just expire.
        /// </summary>
        private async Task ResolveAndSaveAsync(IEnumerable<ChallengeEntity> challenges)
        {
            var now = DateTime.UtcNow;
            var touchedUsers = new HashSet<Guid>();

            foreach (var c in challenges)
            {
                c.ResolvedAt = now;
                if (c.Status == ChallengeStatus.Pending
                    || (c.ChallengerScore is null && c.OpponentScore is null))
                {
                    c.Status = ChallengeStatus.Expired;
                    c.ChallengerSeenResult = c.OpponentSeenResult = true; // nothing to celebrate
                    continue;
                }

                c.Status = ChallengeStatus.Completed;
                var result = DuelRules.Compare(c.ChallengerScore, c.OpponentScore, c.Contest, c.Scoring);
                c.WinnerId = result > 0 ? c.ChallengerId : result < 0 ? c.OpponentId : null;
                c.ChallengerXP = XPFor(result, c.ChallengerScore);
                c.OpponentXP = XPFor(-result, c.OpponentScore);

                await GrantAsync(c.ChallengerId, c.ChallengerXP, result, c.ChallengerScore is not null);
                await GrantAsync(c.OpponentId, c.OpponentXP, -result, c.OpponentScore is not null);
                touchedUsers.Add(c.ChallengerId);
                touchedUsers.Add(c.OpponentId);
            }

            await _db.SaveChangesAsync();
            foreach (var user in touchedUsers)
                await _achievements.CheckAndUnlockAsync(user);
        }

        /// <summary>No-shows (no score) get nothing; everyone who competed gets something.</summary>
        private static int XPFor(int result, float? score) =>
            score is null ? 0 : result > 0 ? DuelRules.WinXP : result == 0 ? DuelRules.TieXP : DuelRules.LossXP;

        /// <summary>XP and gold for one side; <paramref name="result"/> is +1 win, 0 tie, -1 loss.</summary>
        private async Task GrantAsync(Guid userId, int xp, int result, bool competed)
        {
            if (!competed) return;
            var stats = await _goals.GetOrCreateStatsAsync(userId);
            _goals.GrantXP(stats, xp, result > 0 ? XPReason.FriendChallengeWon : XPReason.DuelPlayed);
            _goals.GrantGold(stats, GoldRules.ForDuel(result > 0 ? "Won" : result == 0 ? "Tie" : "Lost", competed));
        }

        /// <summary>Stat points for participants not already handled by the caller.</summary>
        private async Task ApplyOpponentStatsAsync(ChallengeEntity c, Guid? exceptUserId)
        {
            if (c.Status != ChallengeStatus.Completed) return;
            if (c.ChallengerId != exceptUserId && c.ChallengerXP > 0)
                await _character.ApplyActivityAsync(c.ChallengerId, StatRules.ForDuel(c.Contest, c.ChallengerXP));
            if (c.OpponentId != exceptUserId && c.OpponentXP > 0)
                await _character.ApplyActivityAsync(c.OpponentId, StatRules.ForDuel(c.Contest, c.OpponentXP));
        }

        // ── Loading / mapping ───────────────────────────────────────────────

        private IQueryable<ChallengeEntity> Query() => _db.Challenges
            .Include(c => c.Challenger)
            .Include(c => c.Opponent)
            .Include(c => c.Attempts);

        private async Task<ChallengeEntity> LoadAsync(Guid userId, Guid challengeId) =>
            await Query().FirstOrDefaultAsync(c => c.ChallengeId == challengeId
                    && (c.ChallengerId == userId || c.OpponentId == userId))
                ?? throw new KeyNotFoundException("Challenge not found.");

        /// <summary>Loads a challenge, settling it first if its deadline has passed.</summary>
        private async Task<ChallengeEntity> LoadOpenAsync(Guid userId, Guid challengeId)
        {
            var challenge = await LoadAsync(userId, challengeId);
            if (challenge.Deadline <= DateTime.UtcNow
                && challenge.Status is ChallengeStatus.Pending or ChallengeStatus.Active)
            {
                await ResolveAndSaveAsync(new[] { challenge });
                await ApplyOpponentStatsAsync(challenge, exceptUserId: null);
                throw new InvalidOperationException("This challenge has ended.");
            }
            return challenge;
        }

        private static ChallengeDto ToDto(ChallengeEntity c, Guid userId)
        {
            var me = c.ChallengerId == userId;
            var them = me ? c.Opponent : c.Challenger;
            var myAttempts = c.Attempts.Count(a => a.UserId == userId);
            var open = c.Deadline > DateTime.UtcNow;

            return new ChallengeDto
            {
                ChallengeId = c.ChallengeId,
                Contest = c.Contest.ToString(),
                Scoring = c.Scoring.ToString(),
                DistanceKm = c.DistanceKm,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt,
                Deadline = c.Deadline,
                IsChallenger = me,
                OpponentUserId = them.UserId,
                OpponentUsername = them.Username,
                MyScore = me ? c.ChallengerScore : c.OpponentScore,
                TheirScore = me ? c.OpponentScore : c.ChallengerScore,
                MyAttempts = myAttempts,
                TheirAttempts = c.Attempts.Count - myAttempts,
                Outcome = c.Status != ChallengeStatus.Completed ? null
                    : c.WinnerId is null ? "Tie"
                    : c.WinnerId == userId ? "Won" : "Lost",
                XPEarned = me ? c.ChallengerXP : c.OpponentXP,
                ResultSeen = c.Status != ChallengeStatus.Completed
                    || (me ? c.ChallengerSeenResult : c.OpponentSeenResult),
                CanRespond = !me && open && c.Status == ChallengeStatus.Pending,
                CanCancel = me && open && c.Status == ChallengeStatus.Pending,
                CanSubmit = open && c.Status == ChallengeStatus.Active
                    && (c.Scoring == DuelScoring.Total || myAttempts == 0)
            };
        }

        private static CompletedGoalDto ToGoal(ChallengeEntity c, ChallengeDto dto) => new()
        {
            Kind = DuelKind,
            Id = c.ChallengeId,
            Title = $"{DuelRules.ContestName(c.Contest)} vs {dto.OpponentUsername} — {dto.Outcome}",
            XPReward = dto.XPEarned
        };

        private static bool TryParseEnum<T>(string value, out T result) where T : struct, Enum =>
            Enum.TryParse(value, true, out result) && Enum.IsDefined(result);
    }
}
