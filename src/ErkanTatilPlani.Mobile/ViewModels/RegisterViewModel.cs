using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _phone = string.Empty;
    private string _errorMessage = string.Empty;

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

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

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public RegisterViewModel()
    {
        _apiService = new ApiService();
        Title = "Kayit Ol";
        RegisterCommand = new Command(async () => await RegisterAsync());
        GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Ad ve soyad gereklidir";
            return;
        }

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email ve sifre gereklidir";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Sifre en az 6 karakter olmalidir";
            return;
        }

        try
        {
            IsBusy = true;
            var (success, error) = await _apiService.RegisterAsync(FirstName, LastName, Email, Password,
                string.IsNullOrWhiteSpace(Phone) ? null : Phone);

            if (success)
            {
                Application.Current!.MainPage = new AppShell();
            }
            else
            {
                ErrorMessage = error ?? "Kayit basarisiz";
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
