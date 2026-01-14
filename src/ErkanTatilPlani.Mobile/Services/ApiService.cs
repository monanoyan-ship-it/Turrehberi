using System.Net.Http.Json;
using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://localhost:7001/api"; // Development URL

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    // Companies
    public async Task<List<Company>> GetCompaniesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Company>>($"{BaseUrl}/companies");
        return response ?? new List<Company>();
    }

    public async Task<Company?> GetCompanyAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Company>($"{BaseUrl}/companies/{id}");
    }

    // Tours
    public async Task<List<Tour>> GetToursAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Tour>>($"{BaseUrl}/tours");
        return response ?? new List<Tour>();
    }

    public async Task<Tour?> GetTourAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Tour>($"{BaseUrl}/tours/{id}");
    }

    // Visitors
    public async Task<List<Visitor>> GetVisitorsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Visitor>>($"{BaseUrl}/visitors");
        return response ?? new List<Visitor>();
    }

    public async Task<bool> CreateVisitorAsync(Visitor visitor)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/visitors", visitor);
        return response.IsSuccessStatusCode;
    }

    // Reservations
    public async Task<List<Reservation>> GetReservationsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Reservation>>($"{BaseUrl}/reservations");
        return response ?? new List<Reservation>();
    }

    public async Task<bool> CreateReservationAsync(Reservation reservation)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/reservations", reservation);
        return response.IsSuccessStatusCode;
    }
}
