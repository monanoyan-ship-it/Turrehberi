using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class SupportViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _subject = string.Empty;
    private string _message = string.Empty;
    private string _resultMessage = string.Empty;
    private bool _isSent;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Subject { get => _subject; set => SetProperty(ref _subject, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public string ResultMessage { get => _resultMessage; set => SetProperty(ref _resultMessage, value); }
    public bool IsSent { get => _isSent; set => SetProperty(ref _isSent, value); }

    public ICommand SendCommand { get; }

    public SupportViewModel()
    {
        Title = "Destek";
        _apiService = new ApiService();
        Name = _apiService.UserName ?? string.Empty;
        SendCommand = new Command(async () => await SendAsync());
    }

    private async Task SendAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Message))
        {
            ResultMessage = "Lutfen tum alanlari doldurun";
            return;
        }

        try
        {
            IsBusy = true;
            ResultMessage = string.Empty;

            var (success, error) = await _apiService.SendSupportMessageAsync(Name, Email, Subject, Message);
            if (success)
            {
                IsSent = true;
                ResultMessage = "Mesajiniz basariyla gonderildi. En kisa surede donus yapacagiz.";
                Name = string.Empty;
                Email = string.Empty;
                Subject = string.Empty;
                Message = string.Empty;
            }
            else
            {
                ResultMessage = error ?? "Mesaj gonderilemedi";
            }
        }
        catch (Exception ex)
        {
            ResultMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
