using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Chat;
using MDRNJfitness.Models.Social;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>Admins: reported chat messages. DELETE removes the message; KEEP clears its reports.</summary>
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
        public Task OpenSenderAsync(ReportedMessageCard card) =>
            Shell.Current.GoToAsync($"FriendProfilePage?userId={card.Report.SenderId}");

        private void Drop(ReportedMessageCard card)
        {
            Reports.Remove(card);
            HasNoReports = Reports.Count == 0;
        }
    }
}
