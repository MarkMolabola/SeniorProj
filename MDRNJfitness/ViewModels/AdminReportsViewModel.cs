using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Chat;
using MDRNJfitness.Models.Social;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>
    /// Admins: reported chat messages. DELETE removes the message; KEEP clears its reports;
    /// SENDER mutes or bans whoever wrote it.
    /// </summary>
    public partial class AdminReportsViewModel : BaseViewModel
    {
        private readonly IChatService _chat;

        public ObservableCollection<ReportedMessageCard> Reports { get; } = new();
        [ObservableProperty] private bool _hasNoReports;

        public AdminReportsViewModel(IChatService chat)
        {
            _chat = chat;
            Title = "Reports";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var reports = await _chat.GetReportsAsync();
                Reports.Clear();
                foreach (var r in reports) Reports.Add(new ReportedMessageCard(r));
                HasNoReports = Reports.Count == 0;
            });

        [RelayCommand]
        public async Task DeleteAsync(ReportedMessageCard card) =>
            await RunSafeAsync(async () =>
            {
                if (!await Shell.Current.DisplayAlertAsync("Delete message",
                        $"Delete {card.Report.SenderName}'s message for everyone?", "Delete", "Cancel"))
                    return;
                await _chat.DeleteMessageAsync(card.Report.MessageId);
                Drop(card);
            });

        [RelayCommand]
        public async Task KeepAsync(ReportedMessageCard card) =>
            await RunSafeAsync(async () =>
            {
                await _chat.DismissReportsAsync(card.Report.MessageId);
                Drop(card);
            });

        [RelayCommand]
        public async Task ModerateSenderAsync(ReportedMessageCard card)
        {
            var r = card.Report;
            const string Mute1 = "Mute 1 hour", Mute24 = "Mute 24 hours", Mute7 = "Mute 7 days",
                Unmute = "Lift mute", Ban = "Ban account", Unban = "Unban account";

            var options = new List<string> { Mute1, Mute24, Mute7 };
            if (r.SenderMutedUntil is not null) options.Add(Unmute);
            options.Add(r.SenderBanned ? Unban : Ban);

            var choice = await Shell.Current.DisplayActionSheetAsync(r.SenderName, "Cancel", null, options.ToArray());
            if (choice is null or "Cancel") return;

            if (choice == Ban && !await Shell.Current.DisplayAlertAsync("Ban account",
                    $"Ban {r.SenderName}? They're signed out everywhere and can't sign in or chat until you unban them.",
                    "Ban", "Cancel"))
                return;

            await RunSafeAsync(async () =>
            {
                await (choice switch
                {
                    Mute1 => _chat.MuteUserAsync(r.SenderId, 1),
                    Mute24 => _chat.MuteUserAsync(r.SenderId, 24),
                    Mute7 => _chat.MuteUserAsync(r.SenderId, 24 * 7),
                    Unmute => _chat.MuteUserAsync(r.SenderId, 0),
                    Ban => _chat.SetBannedAsync(r.SenderId, true),
                    _ => _chat.SetBannedAsync(r.SenderId, false)
                });
            });
            await LoadAsync();
        }

        [RelayCommand]
        public Task OpenSenderAsync(ReportedMessageCard card) =>
            Shell.Current.GoToAsync($"FriendProfilePage?userId={card.Report.SenderId}");

        private void Drop(ReportedMessageCard card)
        {
            Reports.Remove(card);
            HasNoReports = Reports.Count == 0;
        }
    }
}
