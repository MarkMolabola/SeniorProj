using CommunityToolkit.Maui;
using MDRNJfitness.Data;
using MDRNJfitness.Helpers;
using MDRNJfitness.Services;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.SignalR;
using MDRNJfitness.ViewModels;
using MDRNJfitness.Views;
using Microsoft.Extensions.Logging;

namespace MDRNJfitness
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("PressStart2P-Regular.ttf", "PixelHeading");
                    fonts.AddFont("VT323-Regular.ttf", "PixelBody");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Data layer
            builder.Services.AddSingleton<LocalDatabase>();
            builder.Services.AddSingleton<WorkoutRepo>();
            builder.Services.AddSingleton<CacheRepo>();
            builder.Services.AddSingleton<UserRepo>();

            // HTTP handler
            builder.Services.AddTransient<AuthHeaderHandler>();

            // HTTP services (typed clients with auth handler)
            builder.Services.AddHttpClient<IAuthService, AuthService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IGameService, GameService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IWorkoutService, WorkoutService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<ILeaderboardService, LeaderboardService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IChallengeService, ChallengeService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<ICharacterService, CharacterService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IFriendService, FriendService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IUserService, UserService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IQuestService, QuestService>(
                c => c.BaseAddress = new Uri(ApiConstants.BaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            // Reward / evolution popup after workouts
            builder.Services.AddSingleton<ICharacterProgressPresenter, CharacterProgressPresenter>();

            // Sync service
            builder.Services.AddTransient<ISyncService, SyncService>();

            // SignalR hub client
            builder.Services.AddSingleton<LeaderboardHubClient>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<WorkoutViewModel>();
            builder.Services.AddTransient<CharacterViewModel>();
            builder.Services.AddTransient<ChallengeViewModel>();
            builder.Services.AddTransient<LeaderboardViewModel>();
            builder.Services.AddTransient<FriendsViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<FriendProfileViewModel>();
            builder.Services.AddTransient<CalendarViewModel>();
            builder.Services.AddTransient<QuestViewModel>();
            builder.Services.AddTransient<AdminQuestViewModel>();

            // Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<WorkoutPage>();
            builder.Services.AddTransient<CharacterPage>();
            builder.Services.AddTransient<ChallengePage>();
            builder.Services.AddTransient<LeaderboardPage>();
            builder.Services.AddTransient<FriendsPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<FriendProfilePage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<QuestPage>();
            builder.Services.AddTransient<AdminQuestPage>();

            return builder.Build();
        }
    }
}
