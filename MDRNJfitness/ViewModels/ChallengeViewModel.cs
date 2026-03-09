using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views
{
    public partial class ChallengePage : ContentPage
    {
        public ChallengePage()
        {
            InitializeComponent();
            BindingContext = new ChallengeViewModel();

            Title = "Weekly Quests";
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = true
            });
        }
    }
}
