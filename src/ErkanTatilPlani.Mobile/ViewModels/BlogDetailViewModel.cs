using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

[QueryProperty(nameof(BlogId), "blogId")]
public class BlogDetailViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private int _blogId;
    private string _blogTitle = string.Empty;
    private string _content = string.Empty;
    private string _authorName = string.Empty;
    private string _createdDate = string.Empty;

    public int BlogId
    {
        get => _blogId;
        set
        {
            SetProperty(ref _blogId, value);
            LoadCommand.Execute(null);
        }
    }

    public string BlogTitle { get => _blogTitle; set => SetProperty(ref _blogTitle, value); }
    public string Content { get => _content; set => SetProperty(ref _content, value); }
    public string AuthorName { get => _authorName; set => SetProperty(ref _authorName, value); }
    public string CreatedDate { get => _createdDate; set => SetProperty(ref _createdDate, value); }

    public ICommand LoadCommand { get; }

    public BlogDetailViewModel()
    {
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadBlogAsync());
    }

    private async Task LoadBlogAsync()
    {
        if (IsBusy || BlogId == 0) return;

        try
        {
            IsBusy = true;
            var blog = await _apiService.GetBlogAsync(BlogId);
            if (blog.HasValue)
            {
                var b = blog.Value;
                BlogTitle = GetString(b, "title");
                Title = BlogTitle;
                Content = GetString(b, "content");
                AuthorName = GetString(b, "authorName");
                var created = GetString(b, "createdAt");
                CreatedDate = created.Length >= 10 ? created.Substring(0, 10) : created;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Blog detayi yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
}
