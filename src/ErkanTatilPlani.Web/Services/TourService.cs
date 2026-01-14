using System.Net.Http.Json;
using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public class TourService : ITourService
{
    private readonly HttpClient _httpClient;

    public TourService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Tour>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<Tour>>("api/tours");
        return response ?? Enumerable.Empty<Tour>();
    }

    public async Task<Tour?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Tour>($"api/tours/{id}");
    }

    public async Task<Tour?> CreateAsync(Tour tour)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tours", tour);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Tour>();
        }
        return null;
    }

    public async Task<bool> UpdateAsync(int id, Tour tour)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/tours/{id}", tour);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/tours/{id}");
        return response.IsSuccessStatusCode;
    }
}
