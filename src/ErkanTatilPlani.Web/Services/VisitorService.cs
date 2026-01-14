using System.Net.Http.Json;
using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public class VisitorService : IVisitorService
{
    private readonly HttpClient _httpClient;

    public VisitorService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Visitor>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<Visitor>>("api/visitors");
        return response ?? Enumerable.Empty<Visitor>();
    }

    public async Task<Visitor?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Visitor>($"api/visitors/{id}");
    }

    public async Task<Visitor?> CreateAsync(Visitor visitor)
    {
        var response = await _httpClient.PostAsJsonAsync("api/visitors", visitor);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Visitor>();
        }
        return null;
    }

    public async Task<bool> UpdateAsync(int id, Visitor visitor)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/visitors/{id}", visitor);
        return response.IsSuccessStatusCode;
    }
}
