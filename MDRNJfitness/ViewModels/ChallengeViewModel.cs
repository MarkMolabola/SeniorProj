using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Challenge;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using MDRNJfitness.Models.Social;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.Views;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>1v1 friend challenges: issue, answer, compete, see results.</summary>
    public partial class ChallengeViewModel : BaseViewModel
    {
        private const string DuelKind = "Duel";

        private readonly IChallengeService _challenges;
        private readonly IFriendService _friends;
        private readonly ICharacterProgressPresenter _progress;

        public ObservableCollection<ChallengeCard> Incoming { get; } = new();
        public ObservableCollection<ChallengeCard> Active { get; } = new();
        public ObservableCollection<ChallengeCard> Outgoing { get; } = new();
        public ObservableCollection<ChallengeCard> History { get; } = new();
        public ObservableCollection<FriendDto> Friends { get; } = new();

        [ObservableProperty] private bool _hasIncoming;
        [ObservableProperty] private bool _hasActive;
        [ObservableProperty] private bool _hasOutgoing;
        [ObservableProperty] private bool _hasHistory;
        [ObservableProperty] private bool _hasNothing;

        // ── New challenge form ──────────────────────────────────────────────
        public List<string> Contests { get; } = new() { "Push-ups", "Pull-ups", "Plank", "Run" };
        public List<string> ScoringModes { get; } = new() { "Best attempt", "Total by deadline" };
        public List<string> Durations { get; } = new() { "1 day", "3 days", "7 days" };

        [ObservableProperty] private bool _isFormOpen;
        [ObservableProperty] private FriendDto? _formFriend;
        [ObservableProperty] private int _formContestIndex;
        [ObservableProperty] private int _formScoringIndex;
        [ObservableProperty] private int _formDurationIndex = 1;
        [ObservableProperty] private string _formDistanceKm = "1.6";
        [ObservableProperty] private bool _formNeedsDistance;
        [ObservableProperty] private string _formRulesText = string.Empty;

        public ChallengeViewModel(IChallengeService challenges, IFriendService friends, ICharacterProgressPresenter progress)
        {
            _challenges = challenges;
            _friends = friends;
            _progress = progress;
            Title = "Challenges";
            UpdateFormRules();
        }

        private DuelContest FormContest => (DuelContest)Math.Max(0, FormContestIndex);
        private DuelScoring FormScoring => (DuelScoring)Math.Max(0, FormScoringIndex);

        partial void OnFormContestIndexChanged(int value) => UpdateFormRules();
        partial void OnFormScoringIndexChanged(int value) => UpdateFormRules();

        private void UpdateFormRules()
        {
            FormNeedsDistance = DuelRules.IsTimedRace(FormContest, FormScoring);
            FormRulesText = new ChallengeCard(new ChallengeDto
            {
                Contest = FormContest.ToString(),
                Scoring = FormScoring.ToString(),
                OpponentUsername = "",
                Status = "Pending"
            }).ModeText
            + (DuelRules.UsesStopwatch(FormContest, FormScoring) ? " · timed in-app" : "");
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var all = await _challenges.GetMyChallengesAsync();
                Show(all);
                await ShowUnseenResultsAsync(all);
            });

        private void Show(List<ChallengeDto> all)
        {
            Fill(Incoming, all.Where(c => c.CanRespond));
            Fill(Active, all.Where(c => c.Status == "Active"));
            Fill(Outgoing, all.Where(c => c.Status == "Pending" && c.IsChallenger));
            Fill(History, all.Where(c => c.Status is not ("Pending" or "Active")).Take(20));

            HasIncoming = Incoming.Count > 0;
            HasActive = Active.Count > 0;
            HasOutgoing = Outgoing.Count > 0;
            HasHistory = History.Count > 0;
            HasNothing = all.Count == 0;
        }

        private static void Fill(ObservableCollection<ChallengeCard> target, IEnumerable<ChallengeDto> items)
        {
            target.Clear();
            foreach (var c in items) target.Add(new ChallengeCard(c));
        }

        /// <summary>Results settled while the user was away (opponent finished, deadline passed).</summary>
        private async Task ShowUnseenResultsAsync(List<ChallengeDto> all)
        {
            foreach (var c in all.Where(c => !c.ResultSeen))
            {
                var card = new ChallengeCard(c);
                await _progress.PresentAsync(null, new AccountProgressDto
                {
                    XPGained = c.XPEarned,
                    CompletedGoals = { ToGoal(card) }
                }, Headline(c.Outcome));
                await _challenges.MarkResultSeenAsync(c.ChallengeId);
            }
        }

        // ── New challenge ───────────────────────────────────────────────────

        [RelayCommand]
        public async Task OpenFormAsync() =>
            await RunSafeAsync(async () =>
            {
                var friends = await _friends.GetFriendsAsync();
                if (friends.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("No friends yet",
                        "Add a friend on the Friends tab, then challenge them here.", "OK");
                    return;
                }

                Friends.Clear();
                foreach (var f in friends) Friends.Add(f);
                FormFriend = Friends[0];
                IsFormOpen = true;
            });

        [RelayCommand]
        public void CloseForm() => IsFormOpen = false;

        [RelayCommand]
        public async Task SendChallengeAsync() =>
            await RunSafeAsync(async () =>
            {
                if (FormFriend is null) return;

                float? distance = null;
                if (FormNeedsDistance)
                {
                    if (!float.TryParse(FormDistanceKm, out var km) || km < 0.1f || km > 100)
                    {
                        await Shell.Current.DisplayAlertAsync("Distance", "Enter a race distance between 0.1 and 100 km.", "OK");
                        return;
                    }
                    distance = km;
                }

                await _challenges.CreateAsync(new CreateChallengeDto
                {
                    OpponentId = FormFriend.UserId,
                    Contest = FormContest.ToString(),
                    Scoring = FormScoring.ToString(),
                    DistanceKm = distance,
                    DurationHours = DuelRules.DurationHours[Math.Clamp(FormDurationIndex, 0, DuelRules.DurationHours.Length - 1)]
                });

                IsFormOpen = false;
                Show(await _challenges.GetMyChallengesAsync());
                await Shell.Current.DisplayAlertAsync("Challenge sent!", $"Waiting for {FormFriend.Username} to accept.", "OK");
            });

        // ── Responding ──────────────────────────────────────────────────────

        [RelayCommand]
        public Task AcceptAsync(ChallengeCard card) => ActAndReloadAsync(() => _challenges.AcceptAsync(card.Dto.ChallengeId));

        [RelayCommand]
        public Task DeclineAsync(ChallengeCard card) => ActAndReloadAsync(() => _challenges.DeclineAsync(card.Dto.ChallengeId));

        [RelayCommand]
        public async Task CancelAsync(ChallengeCard card)
        {
            var sure = await Shell.Current.DisplayAlertAsync("Cancel challenge?",
                $"Withdraw your challenge to {card.Dto.OpponentUsername}?", "Withdraw", "Keep");
            if (sure)
                await ActAndReloadAsync(() => _challenges.CancelAsync(card.Dto.ChallengeId));
        }

        private async Task ActAndReloadAsync(Func<Task> action) =>
            await RunSafeAsync(async () =>
            {
                await action();
                Show(await _challenges.GetMyChallengesAsync());
            });

        // ── Competing ───────────────────────────────────────────────────────

        [RelayCommand]
        public async Task SubmitAsync(ChallengeCard card)
        {
            var value = await AskForResultAsync(card);
            if (value is not float result) return;

            if (card.Scoring == DuelScoring.BestAttempt)
            {
                var sure = await Shell.Current.DisplayAlertAsync("Lock it in?",
                    $"Submit {DuelRules.Format(result, card.Contest, card.Scoring)}? You only get one attempt.",
                    "Submit", "Back");
                if (!sure) return;
            }

            await RunSafeAsync(async () =>
            {
                var updated = await _challenges.SubmitAttemptAsync(card.Dto.ChallengeId, result);
                Show(await _challenges.GetMyChallengesAsync());

                if (updated.AccountProgress is not null)
                    await _progress.PresentAsync(updated.CharacterProgress, updated.AccountProgress, Headline(updated.Outcome));
                else
                    await Shell.Current.DisplayAlertAsync("Result saved",
                        card.Scoring == DuelScoring.Total
                            ? $"Your total is now {DuelRules.Format(updated.MyScore ?? result, card.Contest, card.Scoring)}."
                            : $"Locked in! Waiting for {card.Dto.OpponentUsername}.",
                        "OK");
            });
        }

        /// <summary>Stopwatch for plank / timed races, a number prompt for reps and km.</summary>
        private static async Task<float?> AskForResultAsync(ChallengeCard card)
        {
            if (DuelRules.UsesStopwatch(card.Contest, card.Scoring))
            {
                var hint = card.Contest == DuelContest.Plank
                    ? "Get into your plank, tap START, and tap STOP when you drop."
                    : $"Tap START as you set off and STOP when you finish {card.Dto.DistanceKm:0.##} km.";
                var stopwatch = new DuelStopwatchPage(card.Title, hint);
                await Shell.Current.Navigation.PushModalAsync(stopwatch);
                return await stopwatch.Result;
            }

            var unit = DuelRules.Unit(card.Contest, card.Scoring);
            var prompt = unit == "km" ? "How many km did you run?" : $"How many {DuelRules.ContestName(card.Contest).ToLowerInvariant()}?";
            var text = await Shell.Current.DisplayPromptAsync(card.Title, prompt, "Submit", "Cancel",
                placeholder: unit, keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(text)) return null;

            if (float.TryParse(text, out var value) && value > 0 && (unit == "km" || value == MathF.Floor(value)))
                return value;

            await Shell.Current.DisplayAlertAsync("Invalid result",
                unit == "km" ? "Enter a distance like 3.5." : "Enter a whole number of reps.", "OK");
            return null;
        }

        private static string Headline(string? outcome) => outcome switch
        {
            "Won" => "VICTORY!",
            "Tie" => "DRAW!",
            _ => "DEFEAT"
        };

        private static CompletedGoalDto ToGoal(ChallengeCard card) => new()
        {
            Kind = DuelKind,
            Id = card.Dto.ChallengeId,
            Title = $"{DuelRules.ContestName(card.Contest)} vs {card.Dto.OpponentUsername} — {card.Dto.Outcome}",
            XPReward = card.Dto.XPEarned
        };
    }
}
