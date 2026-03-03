using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class NotificationItem : BaseViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;

    private bool _isRead;
    public bool IsRead
    {
        get => _isRead;
        set => SetProperty(ref _isRead, value);
    }

    public string DateDisplay => !string.IsNullOrEmpty(CreatedAt) && CreatedAt.Length >= 10
        ? CreatedAt.Substring(0, 10)
        : "";
}

public class NotificationsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<NotificationItem> Notifications { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand MarkReadCommand { get; }

    public NotificationsViewModel()
    {
        Title = "Bildirimler";
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadNotificationsAsync());
        RefreshCommand = new Command(async () => await LoadNotificationsAsync());
        MarkReadCommand = new Command<NotificationItem>(async (item) => await MarkReadAsync(item));
    }

    private async Task LoadNotificationsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Notifications.Clear();

            if (!_apiService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Uyari", "Bildirimlerinizi gormek icin giris yapin", "Tamam");
                return;
            }

            var notifications = await _apiService.GetNotificationsAsync();

            foreach (var n in notifications)
            {
                Notifications.Add(new NotificationItem
                {
                    Id = n.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    Title = n.TryGetProperty("title", out var title) ? title.GetString() ?? "" : "",
                    Message = n.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "",
                    CreatedAt = n.TryGetProperty("createdAt", out var created) ? created.GetString() ?? "" : "",
                    IsRead = n.TryGetProperty("isRead", out var read) && read.GetBoolean()
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Bildirimler yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkReadAsync(NotificationItem item)
    {
        if (item == null || item.IsRead) return;

        try
        {
            var (success, _) = await _apiService.MarkNotificationReadAsync(item.Id);
            if (success)
            {
                item.IsRead = true;
            }
        }
        catch (Exception)
        {
            // Silently fail for mark-read
        }
    }
}
