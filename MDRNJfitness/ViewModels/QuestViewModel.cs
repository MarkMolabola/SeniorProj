using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Quest;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class QuestViewModel : BaseViewModel
    {
        private readonly IQuestService _quests;
        private readonly IAuthService _auth;

        public ObservableCollection<UserQuestDto> Quests { get; } = new();

        [ObservableProperty] private bool _hasNoQuests = true;
        [ObservableProperty] private bool _isAdmin;

        public QuestViewModel(IQuestService quests, IAuthService auth)
        {
            _quests = quests;
            _auth = auth;
            Title = "Quests";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var user = await _auth.GetCurrentUserAsync();
                IsAdmin = user?.IsAdmin ?? false;

                var myQuests = await _quests.GetMyQuestsAsync();
                Quests.Clear();
                foreach (var q in myQuests)
                    Quests.Add(q);
                HasNoQuests = Quests.Count == 0;
            });
    }
}
