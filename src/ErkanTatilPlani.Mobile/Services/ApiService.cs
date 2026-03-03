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

    // === Profile ===

    public async Task<JsonElement?> GetProfileAsync()
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<JsonElement>($"{BaseUrl}/auth/me", JsonOptions);
    }
}
