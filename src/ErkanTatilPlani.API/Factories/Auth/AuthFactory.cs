using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Auth;
using ErkanTatilPlani.Core.Factories.Referrals;
using ErkanTatilPlani.Core.Helpers;
using ErkanTatilPlani.Core.Infrastructure;

namespace ErkanTatilPlani.API.Factories.Auth;

public class AuthFactory : IAuthFactory
{
    private readonly IVisitorEntityService _visitorService;
    private readonly ICompanyEntityService _companyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IReferralFactory _referralFactory;

    public AuthFactory(
        IVisitorEntityService visitorService,
        ICompanyEntityService companyService,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IReferralFactory referralFactory)
    {
        _visitorService = visitorService;
        _companyService = companyService;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _referralFactory = referralFactory;
    }

    public async Task<(bool success, object result, int statusCode)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return (false, new { error = "Validation.EmailPasswordRequired" }, 400);

        var visitor = await _visitorService.GetActiveByEmailAsync(email);
        if (visitor == null)
            return (false, new { error = "Error.InvalidCredentials" }, 401);

        if (!PasswordHelper.VerifyPassword(password, visitor.PasswordHash))
            return (false, new { error = "Error.InvalidCredentials" }, 401);

        var token = _jwtService.GenerateToken(visitor);
        return (true, AuthUserInfoHelper.BuildLoginResponse(token, visitor), 200);
    }

    public async Task<(bool success, object result, int statusCode)> RegisterAsync(
        string firstName, string lastName, string email, string password,
        string? phone, string? identityNumber, string? referralCode = null)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return (false, new { error = "Validation.EmailPasswordRequired" }, 400);

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            return (false, new { error = "Validation.NameRequired" }, 400);

        if (password.Length < 6)
            return (false, new { error = "Validation.PasswordMinLength" }, 400);

        if (await _visitorService.EmailExistsAsync(email))
            return (false, new { error = "Error.EmailAlreadyRegistered" }, 400);

        var visitor = new Visitor
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone ?? string.Empty,
            IdentityNumber = identityNumber ?? string.Empty,
            PasswordHash = PasswordHelper.HashPassword(password),
            UserTypeId = UserTypes.Ids.Visitor,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _visitorService.Add(visitor);
        await _unitOfWork.SaveChangesAsync();

        // Referans kodu uret ve referans isle
        try
        {
            await _referralFactory.GetOrCreateReferralCodeAsync(visitor.Id);
            if (!string.IsNullOrWhiteSpace(referralCode))
                await _referralFactory.ProcessReferralSignupAsync(visitor.Id, referralCode);
        }
        catch { }

        var token = _jwtService.GenerateToken(visitor);
        return (true, AuthUserInfoHelper.BuildLoginResponse(token, visitor), 200);
    }

    public async Task<(bool success, object result, int statusCode)> RegisterCompanyAsync(
        string firstName, string lastName, string email, string password, string? phone,
        string companyName, string taxNumber, string? companyEmail, string? companyPhone,
        string? companyAddress, string? companyWebsite, string? applicationNotes)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return (false, new { error = "Validation.EmailPasswordRequired" }, 400);

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            return (false, new { error = "Validation.NameRequired" }, 400);

        if (password.Length < 6)
            return (false, new { error = "Validation.PasswordMinLength" }, 400);

        if (string.IsNullOrEmpty(companyName))
            return (false, new { error = "Validation.CompanyNameRequired" }, 400);

        if (string.IsNullOrEmpty(taxNumber))
            return (false, new { error = "Validation.TaxNumberRequired" }, 400);

        if (await _visitorService.EmailExistsAsync(email))
            return (false, new { error = "Error.EmailAlreadyRegistered" }, 400);

        var existingCompany = await _companyService.GetByTaxNumberAsync(taxNumber);
        if (existingCompany != null)
            return (false, new { error = "Error.TaxNumberAlreadyRegistered" }, 400);

        var company = new Company
        {
            Name = companyName,
            TaxNumber = taxNumber,
            Email = companyEmail ?? string.Empty,
            Phone = companyPhone ?? string.Empty,
            Address = companyAddress ?? string.Empty,
            Website = companyWebsite ?? string.Empty,
            Description = string.Empty,
            LogoUrl = string.Empty,
            StatusId = CompanyStatuses.Ids.Pending,
            ApplicationDate = DateTime.UtcNow,
            ApplicationNotes = applicationNotes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _companyService.Add(company);
        await _unitOfWork.SaveChangesAsync();

        var visitor = new Visitor
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone ?? string.Empty,
            IdentityNumber = string.Empty,
            PasswordHash = PasswordHelper.HashPassword(password),
            UserTypeId = UserTypes.Ids.CompanyOwner,
            CompanyId = company.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _visitorService.Add(visitor);
        await _unitOfWork.SaveChangesAsync();

        // Set navigation property for UserInfo building
        visitor.Company = company;

        var token = _jwtService.GenerateToken(visitor);
        return (true, AuthUserInfoHelper.BuildLoginResponse(token, visitor), 200);
    }
}
