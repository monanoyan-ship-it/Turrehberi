using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Faqs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqsController : ControllerBase
{
    private readonly IFaqFactory _faqFactory;

    public FaqsController(IFaqFactory faqFactory)
    {
        _faqFactory = faqFactory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
        => Ok(await _faqFactory.GetAllAsync());

    [HttpGet("public")]
    public async Task<ActionResult<IEnumerable<object>>> GetPublic()
        => Ok(await _faqFactory.GetPublicAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Faq>> GetById(int id)
    {
        var faq = await _faqFactory.GetByIdAsync(id);
        if (faq == null) return NotFound();
        return faq;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<Faq>> Create(Faq faq)
    {
        var created = await _faqFactory.CreateAsync(faq);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, Faq faq)
    {
        var (success, message, statusCode) = await _faqFactory.UpdateAsync(id, faq);
        if (!success) return StatusCode(statusCode, new { message });
        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message, statusCode) = await _faqFactory.DeleteAsync(id);
        if (!success) return StatusCode(statusCode, new { message });
        return Ok(new { message });
    }
}
