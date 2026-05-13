using ErkanTatilPlani.Core.Factories.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodFactory _factory;

    public PaymentMethodsController(IPaymentMethodFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("public")]
    public async Task<ActionResult<IEnumerable<object>>> GetPublicMethods()
        => Ok(await _factory.GetPublicMethodsAsync());

    [HttpGet("admin")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<object>>> GetAdminMethods()
        => Ok(await _factory.GetAdminMethodsAsync());

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateMethod([FromBody] PaymentMethodSettingsRequest request)
    {
        var (success, result, statusCode) = await _factory.CreateMethodAsync(request);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateMethod(int id, [FromBody] PaymentMethodSettingsRequest request)
    {
        var (success, result, statusCode) = await _factory.UpdateMethodAsync(id, request);
        return StatusCode(statusCode, result);
    }

    [HttpPost("{id}/set-default")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var (success, result, statusCode) = await _factory.SetDefaultMethodAsync(id);
        return StatusCode(statusCode, result);
    }
}
