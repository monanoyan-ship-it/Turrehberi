namespace ErkanTatilPlani.Core.Factories.Auth;

public interface IAuthFactory
{
    Task<(bool success, object result, int statusCode)> LoginAsync(string email, string password);

    Task<(bool success, object result, int statusCode)> RegisterAsync(
        string firstName, string lastName, string email, string password,
        string? phone, string? identityNumber);

    Task<(bool success, object result, int statusCode)> RegisterCompanyAsync(
        string firstName, string lastName, string email, string password, string? phone,
        string companyName, string taxNumber, string? companyEmail, string? companyPhone,
        string? companyAddress, string? companyWebsite, string? applicationNotes);
}
