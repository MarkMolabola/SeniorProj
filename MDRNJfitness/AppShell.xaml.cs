using MDRNJfitness.Views;

namespace MDRNJfitness
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(FriendProfilePage), typeof(FriendProfilePage));
            Routing.RegisterRoute(nameof(AdminQuestPage), typeof(AdminQuestPage));
            Routing.RegisterRoute(nameof(ChatRoomPage), typeof(ChatRoomPage));
            Routing.RegisterRoute(nameof(AdminReportsPage), typeof(AdminReportsPage));
        }
    }
}
