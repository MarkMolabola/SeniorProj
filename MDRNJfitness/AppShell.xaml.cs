using MDRNJfitness.Views;

namespace MDRNJfitness
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(FriendProfilePage), typeof(FriendProfilePage));
        }
    }
}
