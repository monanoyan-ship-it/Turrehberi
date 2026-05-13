using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Payments;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ErkanTatilPlani.API.Factories.Payments;

public class PaymentMethodFactory : IPaymentMethodFactory
{
    private static readonly HashSet<string> SupportedOnlineProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        "iyzico"
    };

    private readonly IPaymentMethodEntityService _service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public PaymentMethodFactory(
        IPaymentMethodEntityService service,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _service = service;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<IEnumerable<object>> GetPublicMethodsAsync()
    {
        await EnsureDefaultsAsync();

        return await _service.GetActiveMethods()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SystemName,
                x.DisplayName,
                x.Description,
                x.ProviderSystemName,
                x.ProviderDisplayName,
                x.IconClass,
                x.IsDefault,
                x.IsOnline,
                isAvailableForCheckout = x.IsOnline && SupportedOnlineProviders.Contains(x.ProviderSystemName),
                comingSoon = !x.IsOnline || !SupportedOnlineProviders.Contains(x.ProviderSystemName)
            })
            .ToListAsync<object>();
    }

    public async Task<IEnumerable<object>> GetAdminMethodsAsync()
    {
        await EnsureDefaultsAsync();

        return await _service.GetActiveMethods()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SystemName,
                x.DisplayName,
                x.Description,
                x.ProviderSystemName,
                x.ProviderDisplayName,
                x.IsEnabled,
                x.IsDefault,
                x.IsOnline,
                x.SupportsMarketplaceSplit,
                x.DisplayOrder,
                x.IconClass,
                x.ApiKey,
                x.SecretKey,
                x.BaseUrl,
                x.IsSandbox,
                x.ExtraSettingsJson,
                supportsOnlineProvider = SupportedOnlineProviders.Contains(x.ProviderSystemName)
            })
            .ToListAsync<object>();
    }

    public async Task<(bool success, object result, int statusCode)> CreateMethodAsync(PaymentMethodSettingsRequest request)
    {
        var normalized = NormalizeRequest(request);
        if (string.IsNullOrWhiteSpace(normalized.SystemName))
            return (false, new { message = "Error.InvalidRequest" }, 400);

        if (!normalized.IsEnabled)
            normalized.IsDefault = false;

        var exists = await _service.GetBySystemNameAsync(normalized.SystemName);
        if (exists != null)
            return (false, new { message = "Error.RecordAlreadyExists" }, 400);

        var method = ToEntity(normalized);
        _service.Add(method);

        if (method.IsDefault)
            await _service.ClearDefaultFlagsAsync();

        await _unitOfWork.SaveChangesAsync();
        await EnsureSingleDefaultAsync();
        return (true, new { message = "Success.Created", id = method.Id }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> UpdateMethodAsync(int id, PaymentMethodSettingsRequest request)
    {
        var method = await _service.GetByIdAsync(id);
        if (method == null)
            return (false, new { message = "Error.RecordNotFound" }, 404);

        var normalized = NormalizeRequest(request);
        if (string.IsNullOrWhiteSpace(normalized.SystemName))
            return (false, new { message = "Error.InvalidRequest" }, 400);

        if (!normalized.IsEnabled)
            normalized.IsDefault = false;

        var bySystemName = await _service.GetBySystemNameAsync(normalized.SystemName);
        if (bySystemName != null && bySystemName.Id != id)
            return (false, new { message = "Error.RecordAlreadyExists" }, 400);

        if (normalized.IsDefault)
            await _service.ClearDefaultFlagsAsync();

        method.SystemName = normalized.SystemName;
        method.DisplayName = normalized.DisplayName;
        method.Description = normalized.Description;
        method.ProviderSystemName = normalized.ProviderSystemName;
        method.ProviderDisplayName = normalized.ProviderDisplayName;
        method.IsEnabled = normalized.IsEnabled;
        method.IsDefault = normalized.IsDefault;
        method.IsOnline = normalized.IsOnline;
        method.SupportsMarketplaceSplit = normalized.SupportsMarketplaceSplit;
        method.DisplayOrder = normalized.DisplayOrder;
        method.IconClass = normalized.IconClass;
        method.ApiKey = normalized.ApiKey;
        method.SecretKey = normalized.SecretKey;
        method.BaseUrl = normalized.BaseUrl;
        method.IsSandbox = normalized.IsSandbox;
        method.ExtraSettingsJson = normalized.ExtraSettingsJson;
        method.UpdatedAt = DateTime.UtcNow;

        _service.Update(method);
        await _unitOfWork.SaveChangesAsync();
        await EnsureSingleDefaultAsync();

        return (true, new { message = "Success.Updated" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> SetDefaultMethodAsync(int id)
    {
        var method = await _service.GetByIdAsync(id);
        if (method == null)
            return (false, new { message = "Error.RecordNotFound" }, 404);

        if (!method.IsEnabled)
            return (false, new { message = "Error.InvalidRequest" }, 400);

        await _service.ClearDefaultFlagsAsync();
        method.IsDefault = true;
        method.UpdatedAt = DateTime.UtcNow;
        _service.Update(method);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.Updated" }, 200);
    }

    private async Task EnsureDefaultsAsync()
    {
        if (await _service.AnyEnabledMethodAsync())
            return;

        var paymentSection = _configuration.GetSection("Payment");

        _service.Add(new PaymentMethodSetting
        {
            SystemName = "iyzico-card",
            DisplayName = "Kredi/Banka Karti",
            Description = "iyzico ile guvenli odeme",
            ProviderSystemName = "iyzico",
            ProviderDisplayName = "Iyzico",
            IsEnabled = true,
            IsDefault = true,
            IsOnline = true,
            SupportsMarketplaceSplit = true,
            DisplayOrder = 1,
            IconClass = "bi bi-credit-card-2-front",
            ApiKey = paymentSection["ApiKey"] ?? string.Empty,
            SecretKey = paymentSection["SecretKey"] ?? string.Empty,
            BaseUrl = paymentSection["BaseUrl"] ?? "https://sandbox-api.iyzipay.com",
            IsSandbox = bool.TryParse(paymentSection["IsSandbox"], out var isSandbox) && isSandbox
        });

        _service.Add(new PaymentMethodSetting
        {
            SystemName = "bank-transfer",
            DisplayName = "Banka Havale/EFT",
            Description = "Manuel odeme onayi",
            ProviderSystemName = "manual",
            ProviderDisplayName = "Manual",
            IsEnabled = true,
            IsDefault = false,
            IsOnline = false,
            SupportsMarketplaceSplit = false,
            DisplayOrder = 2,
            IconClass = "bi bi-bank"
        });

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureSingleDefaultAsync()
    {
        var methods = await _service.GetActiveMethods()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync();

        if (!methods.Any())
            return;

        var defaultMethod = methods.FirstOrDefault(x => x.IsDefault);
        if (defaultMethod != null)
            return;

        methods[0].IsDefault = true;
        methods[0].UpdatedAt = DateTime.UtcNow;
        _service.Update(methods[0]);
        await _unitOfWork.SaveChangesAsync();
    }

    private static PaymentMethodSettingsRequest NormalizeRequest(PaymentMethodSettingsRequest request)
    {
        request.SystemName = NormalizeSystemName(request.SystemName);
        request.ProviderSystemName = NormalizeSystemName(request.ProviderSystemName);
        request.DisplayName = (request.DisplayName ?? string.Empty).Trim();
        request.Description = (request.Description ?? string.Empty).Trim();
        request.ProviderDisplayName = (request.ProviderDisplayName ?? string.Empty).Trim();
        request.IconClass = string.IsNullOrWhiteSpace(request.IconClass) ? "bi bi-credit-card-2-front" : request.IconClass.Trim();
        request.ApiKey = (request.ApiKey ?? string.Empty).Trim();
        request.SecretKey = (request.SecretKey ?? string.Empty).Trim();
        request.BaseUrl = (request.BaseUrl ?? string.Empty).Trim();
        request.ExtraSettingsJson = (request.ExtraSettingsJson ?? string.Empty).Trim();
        return request;
    }

    private static PaymentMethodSetting ToEntity(PaymentMethodSettingsRequest request)
    {
        return new PaymentMethodSetting
        {
            SystemName = request.SystemName,
            DisplayName = request.DisplayName,
            Description = request.Description,
            ProviderSystemName = request.ProviderSystemName,
            ProviderDisplayName = request.ProviderDisplayName,
            IsEnabled = request.IsEnabled,
            IsDefault = request.IsDefault,
            IsOnline = request.IsOnline,
            SupportsMarketplaceSplit = request.SupportsMarketplaceSplit,
            DisplayOrder = request.DisplayOrder,
            IconClass = request.IconClass,
            ApiKey = request.ApiKey,
            SecretKey = request.SecretKey,
            BaseUrl = request.BaseUrl,
            IsSandbox = request.IsSandbox,
            ExtraSettingsJson = request.ExtraSettingsJson
        };
    }

    private static string NormalizeSystemName(string value)
    {
        return (value ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");
    }
}
