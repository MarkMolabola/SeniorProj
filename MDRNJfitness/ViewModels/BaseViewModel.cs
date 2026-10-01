using CommunityToolkit.Mvvm.ComponentModel;

namespace MDRNJfitness.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        public bool IsNotBusy => !IsBusy;

        protected async Task RunSafeAsync(Func<Task> action)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
