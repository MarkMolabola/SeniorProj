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
    /// <summary>
    /// 1v1 friend challenges: issue, answer, compete, see results.
    /// "?challenge={userId}" (from a friend's profile) opens the form with that friend picked.
    /// </summary>
    [QueryProperty(nameof(ChallengeUserId), "challenge")]
    public partial class ChallengeViewModel : BaseViewModel
    {
        private const string DuelKind = "Duel";

        private readonly IChallengeService _challenges;
        private readonly IFriendService _friends;
        private readonly ICharacterProgressPresenter _progress;
        private readonly ICharacterService _character;

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormProofHint))]
        private bool _formRequireProof;
        [ObservableProperty] private bool _formAllowDisputes = true;

        /// <summary>What each player will have to upload, under the proof toggle.</summary>
        public string FormProofHint => FormRequireProof ? DuelRules.ProofHint(FormContest) : "Results are on the honor system.";

        /// <summary>Shown while a proof uploads (videos can take a while).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUploading))]
        private string _uploadStatus = string.Empty;

        public bool IsUploading => !string.IsNullOrEmpty(UploadStatus);

        /// <summary>Friend to challenge once the page loads (set by navigation).</summary>
        private Guid? _pendingOpponent;

        public string ChallengeUserId
        {
            set => _pendingOpponent = Guid.TryParse(value, out var id) ? id : null;
        }

        public ChallengeViewModel(IChallengeService challenges, IFriendService friends, ICharacterProgressPresenter progress,
            ICharacterService character)
        {
            _challenges = challenges;
            _friends = friends;
            _progress = progress;
            _character = character;
            Title = "Challenges";
            UpdateFormRules();
        }

        private DuelContest FormContest => (DuelContest)Math.Max(0, FormContestIndex);
        private DuelScoring FormScoring => (DuelScoring)Math.Max(0, FormScoringIndex);

        partial void OnFormContestIndexChanged(int value)
        {
            UpdateFormRules();
            OnPropertyChanged(nameof(FormProofHint));
        }
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

                if (_pendingOpponent is Guid opponent)
                {
                    _pendingOpponent = null;
                    await OpenFormForAsync(opponent);
                }
            });

        private void Show(List<ChallengeDto> all)
        {
            Fill(Incoming, all.Where(c => c.CanRespond));
            Fill(Active, all.Where(c => c.Status is "Active" or "InReview"));
            Fill(Outgoing, all.Where(c => c.Status == "Pending" && c.IsChallenger));
            Fill(History, all.Where(c => c.Status is not ("Pending" or "Active" or "InReview")).Take(20));

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
                await PlayDuelAsync(card);
                await _progress.PresentAsync(null, new AccountProgressDto
                {
                    XPGained = c.XPEarned,
                    GoldGained = GoldRules.ForDuel(c.Outcome, competed: c.MyScore is not null),
                    CompletedGoals = { ToGoal(card) }
                }, Headline(c.Outcome));
                await _challenges.MarkResultSeenAsync(c.ChallengeId);
            }
        }

        // ── New challenge ───────────────────────────────────────────────────

        [RelayCommand]
        public async Task OpenFormAsync() =>
            await RunSafeAsync(() => OpenFormForAsync(null));

        /// <summary>Opens the new-challenge form, with <paramref name="friendId"/> picked if given.</summary>
        private async Task OpenFormForAsync(Guid? friendId)
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
            FormFriend = Friends.FirstOrDefault(f => f.UserId == friendId) ?? Friends[0];
            IsFormOpen = true;
        }

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
                    DurationHours = DuelRules.DurationHours[Math.Clamp(FormDurationIndex, 0, DuelRules.DurationHours.Length - 1)],
                    RequireProof = FormRequireProof,
                    AllowDisputes = FormRequireProof && FormAllowDisputes
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
            // With proof: capture it first, then read the result off it (no in-app stopwatch:
            // the phone is busy filming the plank, and a run's time is on its summary)
            FileResult? proof = null;
            if (card.Dto.RequireProof)
            {
                proof = await PickProofAsync(card);
                if (proof is null) return;
            }

            var value = await AskForResultAsync(card, allowStopwatch: proof is null);
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
                string? proofPath = null;
                if (proof is not null)
                {
                    UploadStatus = card.ProofKind == ProofKind.Video ? "UPLOADING VIDEO… KEEP THE APP OPEN" : "UPLOADING PHOTO…";
                    try { proofPath = await _challenges.UploadProofAsync(card.Dto.ChallengeId, proof); }
                    finally { UploadStatus = string.Empty; }
                }

                var updated = await _challenges.SubmitAttemptAsync(card.Dto.ChallengeId, result, proofPath);
                Show(await _challenges.GetMyChallengesAsync());

                if (updated.AccountProgress is not null)
                {
                    await PlayDuelAsync(new ChallengeCard(updated));
                    await _progress.PresentAsync(updated.CharacterProgress, updated.AccountProgress, Headline(updated.Outcome));
                }
                else if (updated.Status == "InReview")
                    await Shell.Current.DisplayAlertAsync("Results are in!",
                        $"Both results are locked in. Check {card.Dto.OpponentUsername}'s proof and approve or dispute it. "
                        + $"The result counts in {DuelRules.ReviewHours}h if nobody disputes.", "OK");
                else
                    await Shell.Current.DisplayAlertAsync("Result saved",
                        card.Scoring == DuelScoring.Total
                            ? $"Your total is now {DuelRules.Format(updated.MyScore ?? result, card.Contest, card.Scoring)}."
                            : $"Locked in! Waiting for {card.Dto.OpponentUsername}.",
                        "OK");
            });
        }

        /// <summary>Video challenges: record now or pick a clip. Run challenges: photo or screenshot of the summary.</summary>
        private static async Task<FileResult?> PickProofAsync(ChallengeCard card)
        {
            var video = card.ProofKind == ProofKind.Video;
            var capture = video ? "Record a video" : "Take a photo";
            var pick = video ? "Choose a video" : "Choose a photo / screenshot";
            var options = MediaPicker.Default.IsCaptureSupported ? new[] { capture, pick } : new[] { pick };

            var choice = await Shell.Current.DisplayActionSheetAsync(
                $"PROOF: {DuelRules.ProofHint(card.Contest)}", "Cancel", null, options);
            if (choice is null || choice == "Cancel") return null;

            try
            {
                if (choice == capture)
                {
                    if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted)
                    {
                        await Shell.Current.DisplayAlertAsync("Camera", "Allow camera access to record your proof, or choose an existing file.", "OK");
                        return null;
                    }
                    if (video && await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                    {
                        await Shell.Current.DisplayAlertAsync("Microphone", "Allow microphone access to record video, or choose an existing clip.", "OK");
                        return null;
                    }
                    return video ? await MediaPicker.Default.CaptureVideoAsync() : await MediaPicker.Default.CapturePhotoAsync();
                }
                var single = new MediaPickerOptions { SelectionLimit = 1 };
                var picked = video ? await MediaPicker.Default.PickVideosAsync(single) : await MediaPicker.Default.PickPhotosAsync(single);
                return picked?.FirstOrDefault();
            }
            catch (Exception ex) when (ex is FeatureNotSupportedException or PermissionException)
            {
                await Shell.Current.DisplayAlertAsync("Proof", ex.Message, "OK");
                return null;
            }
        }

        [RelayCommand]
        public async Task ViewProofAsync(ProofItem proof) =>
            await RunSafeAsync(async () =>
            {
                var link = await _challenges.GetProofLinkAsync(proof.ChallengeId, proof.AttemptId);
                await Browser.Default.OpenAsync(link, BrowserLaunchMode.SystemPreferred);
            });

        [RelayCommand]
        public async Task ApproveAsync(ChallengeCard card)
        {
            var sure = await Shell.Current.DisplayAlertAsync("Approve result?",
                $"You checked {card.Dto.OpponentUsername}'s proof and it's legit?", "Approve", "Back");
            if (sure)
                await ActAndReloadAsync(() => _challenges.ApproveAsync(card.Dto.ChallengeId));
        }

        [RelayCommand]
        public async Task DisputeAsync(ChallengeCard card)
        {
            var reason = await Shell.Current.DisplayPromptAsync("Dispute proof",
                $"This voids the duel: no winner, and nobody gets XP or gold. What's wrong with {card.Dto.OpponentUsername}'s proof? (optional)",
                "Dispute", "Back", "e.g. reps not counted properly", 200);
            if (reason is null) return; // Back
            await ActAndReloadAsync(() => _challenges.DisputeAsync(card.Dto.ChallengeId, reason));
        }

        /// <summary>
        /// Stopwatch for plank / timed races, a number prompt for reps and km. With proof,
        /// times are typed in from the video / summary instead.
        /// </summary>
        private static async Task<float?> AskForResultAsync(ChallengeCard card, bool allowStopwatch = true)
        {
            if (DuelRules.UsesStopwatch(card.Contest, card.Scoring) && !allowStopwatch)
                return await AskForTimeAsync(card);

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

        /// <summary>A time read off the proof: "1:35", "1:02:10" or plain seconds.</summary>
        private static async Task<float?> AskForTimeAsync(ChallengeCard card)
        {
            var prompt = card.Contest == DuelContest.Plank
                ? "How long did you hold the plank? (as in your video, e.g. 1:35)"
                : $"What was your time for {card.Dto.DistanceKm:0.##} km? (as on the summary, e.g. 24:10)";
            var text = await Shell.Current.DisplayPromptAsync(card.Title, prompt, "Submit", "Cancel", placeholder: "m:ss");
            if (string.IsNullOrWhiteSpace(text)) return null;

            if (TryParseTime(text.Trim(), out var seconds) && seconds > 0)
                return seconds;
            await Shell.Current.DisplayAlertAsync("Invalid time", "Enter a time like 1:35 or 1:02:10.", "OK");
            return null;
        }

        internal static bool TryParseTime(string text, out float seconds)
        {
            seconds = 0;
            var parts = text.Split(':');
            if (parts.Length > 3) return false;
            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var n) || n < 0) return false;
                seconds = seconds * 60 + n;
            }
            return true;
        }

        /// <summary>The fight animation for a settled duel: our hero vs theirs, in their real gear.</summary>
        private async Task PlayDuelAsync(ChallengeCard card)
        {
            if (card.Dto.Outcome is not string outcome) return;

            var me = DuelResultPage.Fighter.Plain("YOU");
            try
            {
                var mine = await _character.GetCharacterAsync();
                me = new DuelResultPage.Fighter("YOU", mine.OutfitSlot ?? "default", mine.AvatarId, mine.InDrawOrder());
            }
            catch { /* draw the default hero */ }

            var name = card.Dto.OpponentUsername.ToUpperInvariant();
            var them = DuelResultPage.Fighter.Plain(name);
            try
            {
                var theirs = await _friends.GetFriendProfileAsync(card.Dto.OpponentUserId);
                them = new DuelResultPage.Fighter(name, theirs.SpriteState, theirs.AvatarId, theirs.InDrawOrder());
            }
            catch { /* unfriended or blocked since: still show the fight */ }

            await DuelResultPage.ShowAsync(me, them, outcome, card.Title, card.MyScoreText, card.TheirScoreText);
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
