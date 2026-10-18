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
        private readonly INotifier _notifier;
        private readonly IProofStorage _proofs;

        public ChallengeService(AppDbContext db, ICharacterService character,
            IGoalProgressService goals, IAchievementService achievements, INotifier notifier, IProofStorage proofs)
        {
            _db = db;
            _character = character;
            _goals = goals;
            _achievements = achievements;
            _notifier = notifier;
            _proofs = proofs;
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
                Deadline = DateTime.UtcNow.AddHours(dto.DurationHours),
                RequireProof = dto.RequireProof,
                AllowDisputes = dto.RequireProof && dto.AllowDisputes
            };

            _db.Challenges.Add(challenge);
            await _db.SaveChangesAsync();

            var created = await LoadAsync(userId, challenge.ChallengeId);
            await _notifier.NotifyAsync(created.OpponentId, NotificationType.ChallengeInvite,
                $"{created.Challenger.Username} challenged you to {DuelRules.ContestName(contest)}!", userId, created.ChallengeId);
            return ToDto(created, userId);
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
            await _notifier.NotifyAsync(challenge.ChallengerId, NotificationType.ChallengeAccepted,
                $"{challenge.Opponent.Username} accepted your {DuelRules.ContestName(challenge.Contest)} challenge. Game on!",
                userId, challenge.ChallengeId);
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
            await _notifier.NotifyAsync(challenge.ChallengerId, NotificationType.ChallengeDeclined,
                $"{challenge.Opponent.Username} declined your {DuelRules.ContestName(challenge.Contest)} challenge.",
                userId, challenge.ChallengeId);
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

        public async Task<ChallengeDto> SubmitAttemptAsync(Guid userId, Guid challengeId, float value, string? proofPath)
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

            if (challenge.RequireProof)
                await CheckProofAsync(challenge, userId, proofPath);
            else
                proofPath = null;

            // Added via the DbSet: the attempt's preset key would otherwise make EF treat it as existing
            _db.ChallengeAttempts.Add(new ChallengeAttemptEntity
            {
                ChallengeId = challengeId,
                UserId = userId,
                Value = value,
                ProofPath = proofPath
            });
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

            // Reviewed challenges wait for approval (or the review window) before anyone is paid
            if (challenge.AllowDisputes)
            {
                StartReview(challenge, DateTime.UtcNow);
                await _db.SaveChangesAsync();
                await NotifyReviewAsync(challenge);
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

            // The other side learns the result live; their popup shows when they open Challenges
            var otherId = isChallenger ? challenge.OpponentId : challenge.ChallengerId;
            var submitter = isChallenger ? challenge.Challenger : challenge.Opponent;
            await _notifier.NotifyAsync(otherId, NotificationType.ChallengeCompleted,
                $"Results are in for your {DuelRules.ContestName(challenge.Contest)} duel vs {submitter.Username}!",
                userId, challenge.ChallengeId);
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

        /// <summary>
        /// Expires unanswered challenges, settles running ones past their deadline (or sends
        /// them to review), and settles reviews nobody disputed in time.
        /// </summary>
        private async Task ResolveDueAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var due = await Query()
                .Where(c => (c.ChallengerId == userId || c.OpponentId == userId)
                         && (((c.Status == ChallengeStatus.Pending || c.Status == ChallengeStatus.Active) && c.Deadline <= now)
                             || (c.Status == ChallengeStatus.InReview && c.ReviewDeadline <= now)))
                .ToListAsync();
            await SettleDueAsync(due);

            // Ended challenges whose proofs have outlived their viewing window
            var cutoff = now.AddHours(-ProofKeepHours);
            var stale = await _db.Challenges
                .Include(c => c.Attempts)
                .Where(c => (c.ChallengerId == userId || c.OpponentId == userId)
                         && c.ResolvedAt != null
                         && (c.AllowDisputes || c.ResolvedAt <= cutoff)
                         && c.Attempts.Any(a => a.ProofPath != null))
                .ToListAsync();
            await DeleteExpiredProofsAsync(stale);
        }

        private async Task SettleDueAsync(List<ChallengeEntity> due)
        {
            if (due.Count == 0) return;
            var now = DateTime.UtcNow;

            // Running out the clock on a reviewed challenge opens the review, unless nobody played
            var toReview = due.Where(c => c.Status == ChallengeStatus.Active && c.AllowDisputes
                && (c.ChallengerScore is not null || c.OpponentScore is not null)).ToList();
            foreach (var c in toReview)
                StartReview(c, now);
            if (toReview.Count > 0)
                await _db.SaveChangesAsync();
            foreach (var c in toReview)
                await NotifyReviewAsync(c);

            var toResolve = due.Except(toReview).ToList();
            if (toResolve.Count == 0) return;
            await ResolveAndSaveAsync(toResolve);
            foreach (var challenge in toResolve)
                await ApplyOpponentStatsAsync(challenge, exceptUserId: null);
        }

        private static void StartReview(ChallengeEntity c, DateTime now)
        {
            c.Status = ChallengeStatus.InReview;
            c.ReviewDeadline = now.AddHours(DuelRules.ReviewHours);
        }

        private async Task NotifyReviewAsync(ChallengeEntity c)
        {
            var contest = DuelRules.ContestName(c.Contest);
            await _notifier.NotifyAsync(c.ChallengerId, NotificationType.ChallengeReview,
                $"Results are in for your {contest} duel vs {c.Opponent.Username}. Check their proof!", c.OpponentId, c.ChallengeId);
            await _notifier.NotifyAsync(c.OpponentId, NotificationType.ChallengeReview,
                $"Results are in for your {contest} duel vs {c.Challenger.Username}. Check their proof!", c.ChallengerId, c.ChallengeId);
        }

        // ── Proof and review ────────────────────────────────────────────────

        public async Task<ProofUploadDto> CreateProofUploadAsync(Guid userId, Guid challengeId, string contentType)
        {
            var challenge = await LoadOpenAsync(userId, challengeId);
            if (!challenge.RequireProof)
                throw new InvalidOperationException("This challenge doesn't need proof.");
            if (challenge.Status != ChallengeStatus.Active)
                throw new InvalidOperationException("This challenge isn't running.");

            var kind = DuelRules.ProofKindFor(challenge.Contest);
            var extension = ExtensionFor(contentType, kind)
                ?? throw new ArgumentException(kind == ProofKind.Video
                    ? "Proof for this challenge has to be a video."
                    : "Proof for this challenge has to be a photo.");

            return _proofs.CreateUpload(ProofPaths.ForUpload(challengeId, userId, extension),
                contentType.ToLowerInvariant(), DuelRules.MaxProofBytes(kind));
        }

        /// <summary>The proof must be this player's upload for this challenge, and within the size limit.</summary>
        private async Task CheckProofAsync(ChallengeEntity challenge, Guid userId, string? proofPath)
        {
            if (string.IsNullOrWhiteSpace(proofPath))
                throw new ArgumentException("This challenge needs proof: attach a video or photo of your result. (Update the app if you don't see the option.)");
            if (!proofPath.StartsWith(ProofPaths.UserFolder(challenge.ChallengeId, userId), StringComparison.Ordinal)
                || proofPath.Contains(".."))
                throw new ArgumentException("That proof doesn't belong to this challenge.");
            if (challenge.Attempts.Any(a => a.ProofPath == proofPath))
                throw new InvalidOperationException("That proof was already used for another result.");

            var size = await _proofs.GetSizeAsync(proofPath)
                ?? throw new ArgumentException("The proof upload didn't finish. Try again.");
            var max = DuelRules.MaxProofBytes(DuelRules.ProofKindFor(challenge.Contest));
            if (size > max)
            {
                await _proofs.DeleteAsync(proofPath);
                throw new ArgumentException($"That file is too big (limit {max / (1024 * 1024)} MB). Try a shorter clip.");
            }
        }

        private static string? ExtensionFor(string contentType, ProofKind kind) =>
            (kind, contentType.ToLowerInvariant()) switch
            {
                (ProofKind.Video, "video/mp4") => ".mp4",
                (ProofKind.Video, "video/quicktime") => ".mov",
                (ProofKind.Video, "video/3gpp") => ".3gp",
                (ProofKind.Video, "video/webm") => ".webm",
                (ProofKind.Photo, "image/jpeg") => ".jpg",
                (ProofKind.Photo, "image/png") => ".png",
                (ProofKind.Photo, "image/heic") => ".heic",
                (ProofKind.Photo, "image/webp") => ".webp",
                _ => null
            };

        public async Task<ProofLinkDto> GetProofLinkAsync(Guid userId, Guid challengeId, Guid attemptId)
        {
            var challenge = await LoadAsync(userId, challengeId);
            if (ProofsExpired(challenge, DateTime.UtcNow))
                throw new KeyNotFoundException("That proof has been deleted.");
            var attempt = challenge.Attempts.FirstOrDefault(a => a.AttemptId == attemptId && a.ProofPath is not null
                    && (a.UserId == userId || !TheirResultHidden(challenge)))
                ?? throw new KeyNotFoundException("Proof not found.");

            return new ProofLinkDto
            {
                Url = _proofs.CreateReadUrl(attempt.ProofPath!),
                Kind = DuelRules.ProofKindFor(challenge.Contest).ToString()
            };
        }

        public async Task<ChallengeDto> ApproveAsync(Guid userId, Guid challengeId)
        {
            var challenge = await LoadOpenAsync(userId, challengeId);
            if (challenge.Status != ChallengeStatus.InReview)
                throw new InvalidOperationException("This challenge isn't in review.");

            if (challenge.ChallengerId == userId) challenge.ChallengerApproved = true;
            else challenge.OpponentApproved = true;

            if (!(challenge.ChallengerApproved && challenge.OpponentApproved))
            {
                await _db.SaveChangesAsync();
                return ToDto(challenge, userId);
            }

            // Both happy: settle now. Results show as popups on the next Challenges load.
            await ResolveAndSaveAsync(new[] { challenge });
            await ApplyOpponentStatsAsync(challenge, exceptUserId: null);
            var otherId = challenge.ChallengerId == userId ? challenge.OpponentId : challenge.ChallengerId;
            await _notifier.NotifyAsync(otherId, NotificationType.ChallengeCompleted,
                $"Your {DuelRules.ContestName(challenge.Contest)} duel is settled. See the result!", userId, challengeId);
            return ToDto(challenge, userId);
        }

        public async Task<ChallengeDto> DisputeAsync(Guid userId, Guid challengeId, string? reason)
        {
            var challenge = await LoadOpenAsync(userId, challengeId);
            if (challenge.Status != ChallengeStatus.InReview || !challenge.AllowDisputes)
                throw new InvalidOperationException("This challenge can't be disputed.");

            challenge.Status = ChallengeStatus.Disputed;
            challenge.DisputedById = userId;
            challenge.DisputeReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            challenge.ResolvedAt = DateTime.UtcNow;
            challenge.ChallengerSeenResult = challenge.OpponentSeenResult = true; // no rewards to show
            await _db.SaveChangesAsync();
            await DeleteExpiredProofsAsync(new[] { challenge });

            var me = challenge.ChallengerId == userId ? challenge.Challenger : challenge.Opponent;
            var otherId = challenge.ChallengerId == userId ? challenge.OpponentId : challenge.ChallengerId;
            await _notifier.NotifyAsync(otherId, NotificationType.ChallengeDisputed,
                $"{me.Username} disputed your proof. The {DuelRules.ContestName(challenge.Contest)} duel is void.",
                userId, challengeId);
            return ToDto(challenge, userId);
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

                var contest = DuelRules.ContestName(c.Contest);
                await GrantAsync(c.ChallengerId, c.ChallengerXP, result, c.ChallengerScore is not null,
                    $"{contest} vs {c.Opponent?.Username}");
                await GrantAsync(c.OpponentId, c.OpponentXP, -result, c.OpponentScore is not null,
                    $"{contest} vs {c.Challenger?.Username}");
                touchedUsers.Add(c.ChallengerId);
                touchedUsers.Add(c.OpponentId);
            }

            await _db.SaveChangesAsync();
            foreach (var user in touchedUsers)
                await _achievements.CheckAndUnlockAsync(user);
            await DeleteExpiredProofsAsync(challenges);
        }

        // ── Proof cleanup ───────────────────────────────────────────────────

        /// <summary>How long an unreviewed challenge's proofs stay viewable after it ends.</summary>
        private const int ProofKeepHours = 24;

        /// <summary>
        /// Proofs are only kept while they're useful: a reviewed challenge's go as soon as it
        /// settles (the review window was the time to look), others a day after it ends.
        /// </summary>
        private static bool ProofsExpired(ChallengeEntity c, DateTime now) =>
            c.ResolvedAt is { } resolved
            && (c.AllowDisputes || resolved.AddHours(ProofKeepHours) <= now);

        /// <summary>
        /// Deletes the stored files of challenges whose proofs have expired. A failed delete
        /// leaves the paths in place so the next load retries (the container's lifecycle
        /// rule is the backstop).
        /// </summary>
        private async Task DeleteExpiredProofsAsync(IEnumerable<ChallengeEntity> challenges)
        {
            var now = DateTime.UtcNow;
            var cleared = false;
            foreach (var c in challenges)
            {
                if (!ProofsExpired(c, now) || !c.Attempts.Any(a => a.ProofPath is not null)) continue;
                try
                {
                    await _proofs.DeleteFolderAsync(ProofPaths.ChallengeFolder(c.ChallengeId));
                }
                catch
                {
                    continue;
                }
                foreach (var a in c.Attempts)
                    a.ProofPath = null;
                cleared = true;
            }
            if (cleared)
                await _db.SaveChangesAsync();
        }

        /// <summary>No-shows (no score) get nothing; everyone who competed gets something.</summary>
        private static int XPFor(int result, float? score) =>
            score is null ? 0 : result > 0 ? DuelRules.WinXP : result == 0 ? DuelRules.TieXP : DuelRules.LossXP;

        /// <summary>XP and gold for one side; <paramref name="result"/> is +1 win, 0 tie, -1 loss.</summary>
        private async Task GrantAsync(Guid userId, int xp, int result, bool competed, string description)
        {
            if (!competed) return;
            var outcome = result > 0 ? "Won" : result == 0 ? "Tie" : "Lost";
            var stats = await _goals.GetOrCreateStatsAsync(userId);
            _goals.GrantXP(stats, xp, result > 0 ? XPReason.FriendChallengeWon : XPReason.DuelPlayed);
            _goals.GrantGold(stats, GoldRules.ForDuel(outcome, competed), GoldReason.Duel, $"{description} — {outcome}");
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

        /// <summary>
        /// Loads a challenge, settling it first if its deadline (or review window) has passed.
        /// Throws if that ended it; a challenge that just went to review is returned.
        /// </summary>
        private async Task<ChallengeEntity> LoadOpenAsync(Guid userId, Guid challengeId)
        {
            var challenge = await LoadAsync(userId, challengeId);
            var now = DateTime.UtcNow;
            var due = (challenge.Status is ChallengeStatus.Pending or ChallengeStatus.Active && challenge.Deadline <= now)
                || (challenge.Status == ChallengeStatus.InReview && challenge.ReviewDeadline <= now);
            if (!due) return challenge;

            await SettleDueAsync(new List<ChallengeEntity> { challenge });
            if (challenge.Status != ChallengeStatus.InReview)
                throw new InvalidOperationException("This challenge has ended.");
            return challenge;
        }

        private static ChallengeDto ToDto(ChallengeEntity c, Guid userId)
        {
            var me = c.ChallengerId == userId;
            var them = me ? c.Opponent : c.Challenger;
            var myAttempts = c.Attempts.Count(a => a.UserId == userId);
            var open = c.Deadline > DateTime.UtcNow;
            var iApproved = me ? c.ChallengerApproved : c.OpponentApproved;
            var proofsGone = ProofsExpired(c, DateTime.UtcNow);
            var hideTheirs = TheirResultHidden(c);

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
                TheirScore = hideTheirs ? null : me ? c.OpponentScore : c.ChallengerScore,
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
                    && (c.Scoring == DuelScoring.Total || myAttempts == 0),
                RequireProof = c.RequireProof,
                AllowDisputes = c.AllowDisputes,
                ProofKind = DuelRules.ProofKindFor(c.Contest).ToString(),
                Proofs = c.Attempts
                    .Where(a => a.ProofPath is not null && !proofsGone && (a.UserId == userId || !hideTheirs))
                    .OrderBy(a => a.SubmittedAt)
                    .Select(a => new ChallengeProofDto
                    {
                        AttemptId = a.AttemptId,
                        IsMine = a.UserId == userId,
                        Value = a.Value,
                        SubmittedAt = a.SubmittedAt
                    })
                    .ToList(),
                ReviewDeadline = c.Status == ChallengeStatus.InReview ? c.ReviewDeadline : null,
                CanReview = c.Status == ChallengeStatus.InReview && !iApproved,
                IApproved = iApproved,
                TheyApproved = me ? c.OpponentApproved : c.ChallengerApproved,
                DisputedByMe = c.DisputedById == userId,
                DisputeReason = c.Status == ChallengeStatus.Disputed ? c.DisputeReason : null
            };
        }

        /// <summary>
        /// While a challenge runs, neither side sees the other's score or proof: in a best-attempt
        /// duel you can't aim for their number, and a running total stays a surprise until the deadline.
        /// </summary>
        private static bool TheirResultHidden(ChallengeEntity c) => c.Status == ChallengeStatus.Active;

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
