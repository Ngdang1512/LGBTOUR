namespace SaigonAudioTour.Mobile.Services;

public static class PageAlertExtensions
{
    public static Task<bool> DisplayAlertAsync(this Page page, string title, string message, string accept, string cancel)
    {
        #pragma warning disable CS0618
        var task = MainThread.InvokeOnMainThreadAsync(() => page.DisplayAlert(title, message, accept, cancel));
        #pragma warning restore CS0618
        return task;
    }
}
