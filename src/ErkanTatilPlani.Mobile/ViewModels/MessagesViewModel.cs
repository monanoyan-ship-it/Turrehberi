using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class ConversationItem
{
    public int Id { get; set; }
    public string OtherPartyName { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public string LastMessageDate { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public bool HasUnread => UnreadCount > 0;
}

public class MessageItem
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsMine { get; set; }
    public string SentAt { get; set; } = string.Empty;
    public Color BubbleColor => IsMine ? Color.FromArgb("#1976D2") : Color.FromArgb("#E0E0E0");
    public Color TextColor => IsMine ? Colors.White : Colors.Black;
    public LayoutOptions BubbleAlignment => IsMine ? LayoutOptions.End : LayoutOptions.Start;
}

public class MessagesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ConversationItem> Conversations { get; } = new();

    public ICommand LoadConversationsCommand { get; }
    public ICommand RefreshCommand { get; }

    public MessagesViewModel()
    {
        Title = "Mesajlar";
        _apiService = new ApiService();
        LoadConversationsCommand = new Command(async () => await LoadConversationsAsync());
        RefreshCommand = new Command(async () => await LoadConversationsAsync());
    }

    private async Task LoadConversationsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Conversations.Clear();

            if (!_apiService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Uyari", "Mesajlarinizi gormek icin giris yapin", "Tamam");
                return;
            }

            var conversations = await _apiService.GetConversationsAsync();
            foreach (var c in conversations)
            {
                Conversations.Add(new ConversationItem
                {
                    Id = c.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    OtherPartyName = c.TryGetProperty("otherPartyName", out var n) ? n.GetString() ?? "" : "",
                    LastMessage = c.TryGetProperty("lastMessage", out var lm) ? lm.GetString() ?? "" : "",
                    LastMessageDate = c.TryGetProperty("lastMessageDate", out var d) ? d.GetString()?.Substring(0, 10) ?? "" : "",
                    UnreadCount = c.TryGetProperty("unreadCount", out var uc) ? uc.GetInt32() : 0
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Mesajlar yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
