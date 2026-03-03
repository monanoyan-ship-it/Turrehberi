using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class BlogListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;

    public string DateDisplay => !string.IsNullOrEmpty(CreatedAt) && CreatedAt.Length >= 10
        ? CreatedAt.Substring(0, 10)
        : "";
}

public class BlogViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<BlogListItem> Blogs { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand BlogTappedCommand { get; }

    public BlogViewModel()
    {
        Title = "Blog";
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadBlogsAsync());
        RefreshCommand = new Command(async () => await LoadBlogsAsync());
        BlogTappedCommand = new Command<BlogListItem>(async (blog) => await GoToDetailAsync(blog));
    }

    private async Task LoadBlogsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Blogs.Clear();

            var blogs = await _apiService.GetBlogsAsync();

            foreach (var b in blogs)
            {
                Blogs.Add(new BlogListItem
                {
                    Id = b.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    Title = b.TryGetProperty("title", out var title) ? title.GetString() ?? "" : "",
                    Summary = b.TryGetProperty("summary", out var summary) ? summary.GetString() ?? "" : "",
                    AuthorName = b.TryGetProperty("authorName", out var author) ? author.GetString() ?? "" : "",
                    CreatedAt = b.TryGetProperty("createdAt", out var created) ? created.GetString() ?? "" : ""
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Blog yazilari yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoToDetailAsync(BlogListItem blog)
    {
        if (blog == null) return;
        await Shell.Current.GoToAsync($"BlogDetailPage?blogId={blog.Id}");
    }
}
