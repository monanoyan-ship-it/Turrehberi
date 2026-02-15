using System.Net.Http.Json;
using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public class ReservationService : IReservationService
{
    private readonly HttpClient _httpClient;

    public ReservationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<Reservation>>("api/reservations");
        return response ?? Enumerable.Empty<Reservation>();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Reservation>($"api/reservations/{id}");
    }

    public async Task<Reservation?> CreateAsync(Reservation reservation)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reservations", reservation);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Reservation>();
        }
        return null;
    }

    public async Task<bool> UpdateAsync(int id, Reservation reservation)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/reservations/{id}", reservation);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStatusAsync(int id, int status)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/reservations/{id}/status", status);
        return response.IsSuccessStatusCode;
    }
}
