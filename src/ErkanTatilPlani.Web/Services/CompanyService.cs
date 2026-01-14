using System.Net.Http.Json;
using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public class CompanyService : ICompanyService
{
    private readonly HttpClient _httpClient;

    public CompanyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<Company>>("api/companies");
        return response ?? Enumerable.Empty<Company>();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Company>($"api/companies/{id}");
    }

    public async Task<Company?> CreateAsync(Company company)
    {
        var response = await _httpClient.PostAsJsonAsync("api/companies", company);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Company>();
        }
        return null;
    }

    public async Task<bool> UpdateAsync(int id, Company company)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/companies/{id}", company);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/companies/{id}");
        return response.IsSuccessStatusCode;
    }
}
