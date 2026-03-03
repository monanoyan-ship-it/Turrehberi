using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ErkanTatilPlani.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private static readonly string BaseUrl = DeviceInfo.Platform == DevicePlatform.Android
        ? "https://10.0.2.2:7078/api"    // Android emulator -> host
        : "https://localhost:7078/api";    // iOS/Windows

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true // Dev sertifikasi
        };
        _httpClient = new HttpClient(handler);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // === Auth ===

    public string? Token
    {
        get => Preferences.Get("auth_token", null);
        set
        {
            if (value != null)
                Preferences.Set("auth_token", value);
            else
                Preferences.Remove("auth_token");
        }
    }

    public string? UserName
    {
        get => Preferences.Get("user_name", null);
        set
        {
            if (value != null)
                Preferences.Set("user_name", value);
            else
                Preferences.Remove("user_name");
        }
    }

    public int UserId
    {
        get => Preferences.Get("user_id", 0);
        set => Preferences.Set("user_id", value);
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    private void SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(Token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        else
            _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<(bool success, string? error)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/login",
                new { email, password });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                Token = result.GetProperty("token").GetString();
                UserName = result.GetProperty("firstName").GetString();
                UserId = result.GetProperty("visitorId").GetInt32();
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "Giris basarisiz";
            return (false, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Baglanti hatasi: {ex.Message}");
        }
    }

    public async Task<(bool success, string? error)> RegisterAsync(string firstName, string lastName, string email, string password, string? phone)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register",
                new { firstName, lastName, email, password, phone });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                Token = result.GetProperty("token").GetString();
                UserName = result.GetProperty("firstName").GetString();
                UserId = result.GetProperty("visitorId").GetInt32();
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "Kayit basarisiz";
            return (false, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Baglanti hatasi: {ex.Message}");
        }
    }

    public void Logout()
    {
        Token = null;
        UserName = null;
        UserId = 0;
    }

    // === Tours ===

    public async Task<List<JsonElement>> GetToursAsync(string? search = null)
    {
        SetAuthHeader();
        var url = $"{BaseUrl}/tours";
        if (!string.IsNullOrEmpty(search))
            url += $"?search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>(url, JsonOptions);
        return response ?? new List<JsonElement>();
    }

    public async Task<JsonElement?> GetTourAsync(int id)
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/tours/{id}", JsonOptions);
    }

    // === Reservations ===

    public async Task<List<JsonElement>> GetMyReservationsAsync()
    {
        SetAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>($"{BaseUrl}/reservations/my", JsonOptions);
        return response ?? new List<JsonElement>();
    }

    public async Task<(bool success, string? error)> CreateReservationAsync(object reservationData)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/reservations", reservationData);
            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "Rezervasyon olusturulamadi";
            return (false, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Baglanti hatasi: {ex.Message}");
        }
    }

    // === Promotions ===

    public async Task<JsonElement?> GetFlashSalesAsync()
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/promotions/flash-sales", JsonOptions);
    }

    public async Task<JsonElement?> GetLastMinuteDealsAsync()
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/promotions/last-minute", JsonOptions);
    }

    // === Companies ===

    public async Task<List<JsonElement>> GetCompaniesAsync()
    {
        SetAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>($"{BaseUrl}/companies", JsonOptions);
        return response ?? new List<JsonElement>();
    }

    public async Task<JsonElement?> GetCompanyAsync(int id)
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/companies/{id}", JsonOptions);
    }

    // === Blogs ===

    public async Task<List<JsonElement>> GetBlogsAsync()
    {
        SetAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>($"{BaseUrl}/blogs/public", JsonOptions);
        return response ?? new List<JsonElement>();
    }

    public async Task<JsonElement?> GetBlogAsync(int id)
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/blogs/{id}", JsonOptions);
    }

    // === FAQs ===

    public async Task<List<JsonElement>> GetFaqsAsync()
    {
        SetAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>($"{BaseUrl}/faqs/public", JsonOptions);
        return response ?? new List<JsonElement>();
    }

    // === Favorites ===

    public async Task<List<JsonElement>> GetFavoritesAsync()
    {
        SetAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>($"{BaseUrl}/favorites", JsonOptions);
        return response ?? new List<JsonElement>();
    }

    public async Task<(bool success, string? error)> RemoveFavoriteAsync(int favoriteId)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/favorites/{favoriteId}");
            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "Favori silinemedi";
            return (false, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Baglanti hatasi: {ex.Message}");
        }
    }

    // === Notifications ===

    public async Task<List<JsonElement>> GetNotificationsAsync()
    {
        SetAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>($"{BaseUrl}/notifications", JsonOptions);
        return response ?? new List<JsonElement>();
    }

    public async Task<(bool success, string? error)> MarkNotificationReadAsync(int notificationId)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/notifications/{notificationId}/read", new { });
            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "Bildirim guncellenemedi";
            return (false, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Baglanti hatasi: {ex.Message}");
        }
    }

    // === Messages ===

    public async Task<List<JsonElement>> GetConversationsAsync()
    {
        SetAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<List<JsonElement>>($"{BaseUrl}/messages/conversations", JsonOptions);
        return response ?? new List<JsonElement>();
    }

    // === Support ===

    public async Task<(bool success, string? error)> SendSupportMessageAsync(string name, string email, string? subject, string message)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/support",
                new { name, email, subject, message });
            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "Mesaj gonderilemedi";
            return (false, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Baglanti hatasi: {ex.Message}");
        }
    }

    // === MyCompany ===

    public async Task<JsonElement?> GetMyCompanyDashboardAsync()
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/companies/my/dashboard", JsonOptions);
    }

    // === Profile ===

    public async Task<JsonElement?> GetProfileAsync()
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/auth/me", JsonOptions);
    }
}
