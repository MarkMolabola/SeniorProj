using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Quest;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class AdminQuestViewModel : BaseViewModel
    {
        private readonly IQuestService _quests;

        public ObservableCollection<QuestDto> Quests { get; } = new();

        // Form fields for create/edit
        [ObservableProperty] private string _formTitle = string.Empty;
        [ObservableProperty] private string _formDescription = string.Empty;
        [ObservableProperty] private string _formXPReward = string.Empty;
        [ObservableProperty] private string _formQuestType = "WorkoutCount";
        [ObservableProperty] private string _formTargetValue = string.Empty;
        [ObservableProperty] private DateTime _formStartDate = DateTime.Today;
        [ObservableProperty] private DateTime _formEndDate = DateTime.Today.AddDays(7);

        [ObservableProperty] private QuestDto? _selectedQuest;
        [ObservableProperty] private bool _isEditing;

        public List<string> QuestTypes { get; } = new()
        {
            "WorkoutCount", "Distance", "Streak", "XPRace"
        };

        public AdminQuestViewModel(IQuestService quests)
        {
            _quests = quests;
            Title = "Admin - Quests";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var all = await _quests.GetAllQuestsAsync();
                Quests.Clear();
                foreach (var q in all)
                    Quests.Add(q);
            });

        [RelayCommand]
        public async Task SaveAsync() =>
            await RunSafeAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(FormTitle))
                    throw new InvalidOperationException("Title is required.");

                if (!int.TryParse(FormXPReward, out var xp) || xp <= 0)
                    throw new InvalidOperationException("XP Reward must be a positive number.");

                if (!float.TryParse(FormTargetValue, out var target) || target <= 0)
                    throw new InvalidOperationException("Target value must be a positive number.");

                if (IsEditing && SelectedQuest is not null)
                {
                    var updated = await _quests.UpdateQuestAsync(SelectedQuest.QuestId, new UpdateQuestDto
                    {
                        Title = FormTitle,
                        Description = string.IsNullOrWhiteSpace(FormDescription) ? null : FormDescription,
                        XPReward = xp,
                        QuestType = FormQuestType,
                        TargetValue = target,
                        StartDate = FormStartDate,
                        EndDate = FormEndDate
                    });

                    var index = Quests.IndexOf(Quests.First(q => q.QuestId == updated.QuestId));
                    Quests[index] = updated;
                }
                else
                {
                    var created = await _quests.CreateQuestAsync(new CreateQuestDto
                    {
                        Title = FormTitle,
                        Description = string.IsNullOrWhiteSpace(FormDescription) ? null : FormDescription,
                        XPReward = xp,
                        QuestType = FormQuestType,
                        TargetValue = target,
                        StartDate = FormStartDate,
                        EndDate = FormEndDate
                    });

                    Quests.Insert(0, created);
                }

                ClearForm();
            });

        [RelayCommand]
        public void StartEdit(QuestDto quest)
        {
            SelectedQuest = quest;
            IsEditing = true;
            FormTitle = quest.Title;
            FormDescription = quest.Description ?? string.Empty;
            FormXPReward = quest.XPReward.ToString();
            FormQuestType = quest.QuestType;
            FormTargetValue = quest.TargetValue.ToString();
            FormStartDate = quest.StartDate ?? DateTime.Today;
            FormEndDate = quest.EndDate ?? DateTime.Today.AddDays(7);
        }

        [RelayCommand]
        public async Task DeleteAsync(QuestDto quest) =>
            await RunSafeAsync(async () =>
            {
                await _quests.DeleteQuestAsync(quest.QuestId);
                Quests.Remove(quest);
            });

        [RelayCommand]
        public async Task SendToAllAsync(QuestDto quest) =>
            await RunSafeAsync(async () =>
            {
                await _quests.SendToAllAsync(quest.QuestId);
            });

        [RelayCommand]
        public void CancelEdit() => ClearForm();

        private void ClearForm()
        {
            SelectedQuest = null;
            IsEditing = false;
            FormTitle = string.Empty;
            FormDescription = string.Empty;
            FormXPReward = string.Empty;
            FormQuestType = "WorkoutCount";
            FormTargetValue = string.Empty;
            FormStartDate = DateTime.Today;
            FormEndDate = DateTime.Today.AddDays(7);
        }
    }
}
