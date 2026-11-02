namespace MDRNJfitness.Helpers
{
    public static class ModalPages
    {
        /// <summary>
        /// Shows <paramref name="page"/> as a modal and waits until it has fully closed
        /// (Continue or the Android back button). Waiting for ModalPopped rather than
        /// OnDisappearing matters: pushing the next modal while this one is still popping
        /// leaves Android stuck on a page that can't be dismissed.
        /// </summary>
        public static async Task PushAndWaitAsync(Page page)
        {
            var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnPopped(object? sender, ModalPoppedEventArgs e)
            {
                if (e.Modal == page) closed.TrySetResult();
            }

            var app = Application.Current!;
            app.ModalPopped += OnPopped;
            try
            {
                await Shell.Current.Navigation.PushModalAsync(page);
                await closed.Task;
            }
            finally
            {
                app.ModalPopped -= OnPopped;
            }
        }
    }
}
