using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel()
    {
        _apiService = new ApiService();
        Title = "Giris Yap";
        LoginCommand = new Command(async () => await LoginAsync());
        GoToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("RegisterPage"));
    }

    private async Task LoginAsync()
    {
        if (IsBusy) return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email ve sifre gereklidir";
            return;
        }

        try
        {
            IsBusy = true;
            var (success, error) = await _apiService.LoginAsync(Email, Password);

            if (success)
            {
                Application.Current!.MainPage = new AppShell();
            }
            else
            {
                ErrorMessage = error ?? "Giris basarisiz";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
