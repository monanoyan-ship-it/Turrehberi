using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class FaqItem : BaseViewModel
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
}

public class FaqViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<FaqItem> Faqs { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand ToggleCommand { get; }

    public FaqViewModel()
    {
        Title = "Sikca Sorulan Sorular";
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadFaqsAsync());
        ToggleCommand = new Command<FaqItem>(ToggleFaq);
    }

    private async Task LoadFaqsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Faqs.Clear();

            var faqs = await _apiService.GetFaqsAsync();

            foreach (var f in faqs)
            {
                Faqs.Add(new FaqItem
                {
                    Question = f.TryGetProperty("question", out var q) ? q.GetString() ?? "" : "",
                    Answer = f.TryGetProperty("answer", out var a) ? a.GetString() ?? "" : "",
                    Category = f.TryGetProperty("category", out var cat) ? cat.GetString() ?? "" : "",
                    IsExpanded = false
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"SSS yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ToggleFaq(FaqItem faq)
    {
        if (faq == null) return;
        faq.IsExpanded = !faq.IsExpanded;
    }
}
