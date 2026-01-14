using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompaniesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        return await _context.Companies.Where(c => c.IsActive).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();
        return company;
    }

    [HttpGet("{id}/tours")]
    public async Task<ActionResult<IEnumerable<Tour>>> GetCompanyTours(int id)
    {
        var tours = await _context.Tours
            .Where(t => t.CompanyId == id && t.IsActive)
            .ToListAsync();
        return Ok(tours);
    }

    [HttpPost]
    public async Task<ActionResult<Company>> CreateCompany(Company company)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, Company company)
    {
        if (id != company.Id) return BadRequest();
        company.UpdatedAt = DateTime.UtcNow;
        _context.Entry(company).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();
        company.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
