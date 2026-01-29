using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace ErkanTatilPlani.API.Controllers;

[Route("api/admin/email-accounts")]
[ApiController]
public class EmailAccountsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmailAccountsController> _logger;

    public EmailAccountsController(AppDbContext context, ILogger<EmailAccountsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // DTO'lar
    public class EmailAccountDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public bool IsDefault { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TemplateCount { get; set; }
    }

    public class EmailAccountCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool IsDefault { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class EmailAccountUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string? SmtpPassword { get; set; } // Null ise sifre degistirilmez
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class TestEmailDto
    {
        public string ToEmail { get; set; } = string.Empty;
    }

    // GET: api/admin/email-accounts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailAccountDto>>> GetAll()
    {
        var accounts = await _context.EmailAccounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new EmailAccountDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                SmtpHost = a.SmtpHost,
                SmtpPort = a.SmtpPort,
                SmtpUsername = a.SmtpUsername,
                FromEmail = a.FromEmail,
                FromName = a.FromName,
                EnableSsl = a.EnableSsl,
                IsDefault = a.IsDefault,
                DisplayOrder = a.DisplayOrder,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                TemplateCount = a.EmailTemplates.Count(t => t.IsActive)
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // GET: api/admin/email-accounts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EmailAccountDto>> GetById(int id)
    {
        var account = await _context.EmailAccounts
            .Where(a => a.Id == id && a.IsActive)
            .Select(a => new EmailAccountDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                SmtpHost = a.SmtpHost,
                SmtpPort = a.SmtpPort,
                SmtpUsername = a.SmtpUsername,
                FromEmail = a.FromEmail,
                FromName = a.FromName,
                EnableSsl = a.EnableSsl,
                IsDefault = a.IsDefault,
                DisplayOrder = a.DisplayOrder,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                TemplateCount = a.EmailTemplates.Count(t => t.IsActive)
            })
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound(new { message = "Email hesabi bulunamadi" });

        return Ok(account);
    }

    // POST: api/admin/email-accounts
    [HttpPost]
    public async Task<ActionResult<EmailAccountDto>> Create([FromBody] EmailAccountCreateDto dto)
    {
        // Name benzersizlik kontrolu
        if (await _context.EmailAccounts.AnyAsync(a => a.Name == dto.Name && a.IsActive))
            return BadRequest(new { message = "Bu isimde bir email hesabi zaten mevcut" });

        var account = new EmailAccount
        {
            Name = dto.Name,
            Description = dto.Description,
            SmtpHost = dto.SmtpHost,
            SmtpPort = dto.SmtpPort,
            SmtpUsername = dto.SmtpUsername,
            SmtpPassword = dto.SmtpPassword,
            FromEmail = dto.FromEmail,
            FromName = dto.FromName,
            EnableSsl = dto.EnableSsl,
            IsDefault = dto.IsDefault,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Eger yeni hesap varsayilan olarak isaretlendiyse, diger hesaplarin IsDefault'unu false yap
        if (dto.IsDefault)
        {
            await _context.EmailAccounts
                .Where(a => a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
        }

        _context.EmailAccounts.Add(account);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email account created: {Name}", account.Name);

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, new EmailAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description,
            SmtpHost = account.SmtpHost,
            SmtpPort = account.SmtpPort,
            SmtpUsername = account.SmtpUsername,
            FromEmail = account.FromEmail,
            FromName = account.FromName,
            EnableSsl = account.EnableSsl,
            IsDefault = account.IsDefault,
            DisplayOrder = account.DisplayOrder,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            TemplateCount = 0
        });
    }

    // PUT: api/admin/email-accounts/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmailAccountUpdateDto dto)
    {
        var account = await _context.EmailAccounts.FindAsync(id);
        if (account == null || !account.IsActive)
            return NotFound(new { message = "Email hesabi bulunamadi" });

        // Name benzersizlik kontrolu (kendi haric)
        if (await _context.EmailAccounts.AnyAsync(a => a.Name == dto.Name && a.Id != id && a.IsActive))
            return BadRequest(new { message = "Bu isimde bir email hesabi zaten mevcut" });

        account.Name = dto.Name;
        account.Description = dto.Description;
        account.SmtpHost = dto.SmtpHost;
        account.SmtpPort = dto.SmtpPort;
        account.SmtpUsername = dto.SmtpUsername;
        account.FromEmail = dto.FromEmail;
        account.FromName = dto.FromName;
        account.EnableSsl = dto.EnableSsl;
        account.DisplayOrder = dto.DisplayOrder;
        account.UpdatedAt = DateTime.UtcNow;

        // Sifre sadece dolu ise guncelle
        if (!string.IsNullOrWhiteSpace(dto.SmtpPassword))
        {
            account.SmtpPassword = dto.SmtpPassword;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email account updated: {Name}", account.Name);

        return Ok(new { message = "Email hesabi guncellendi" });
    }

    // DELETE: api/admin/email-accounts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _context.EmailAccounts.FindAsync(id);
        if (account == null || !account.IsActive)
            return NotFound(new { message = "Email hesabi bulunamadi" });

        // Varsayilan hesap silinemez
        if (account.IsDefault)
            return BadRequest(new { message = "Varsayilan email hesabi silinemez" });

        // Soft delete
        account.IsActive = false;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email account deleted: {Name}", account.Name);

        return Ok(new { message = "Email hesabi silindi" });
    }

    // PUT: api/admin/email-accounts/5/set-default
    [HttpPut("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var account = await _context.EmailAccounts.FindAsync(id);
        if (account == null || !account.IsActive)
            return NotFound(new { message = "Email hesabi bulunamadi" });

        // Diger hesaplarin IsDefault'unu false yap
        await _context.EmailAccounts
            .Where(a => a.IsDefault && a.Id != id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));

        account.IsDefault = true;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email account set as default: {Name}", account.Name);

        return Ok(new { message = "Varsayilan email hesabi ayarlandi" });
    }

    // POST: api/admin/email-accounts/5/copy
    [HttpPost("{id}/copy")]
    public async Task<ActionResult<EmailAccountDto>> Copy(int id)
    {
        var source = await _context.EmailAccounts.FindAsync(id);
        if (source == null || !source.IsActive)
            return NotFound(new { message = "Email hesabi bulunamadi" });

        // Benzersiz isim olustur
        var baseName = source.Name + " (Kopya)";
        var newName = baseName;
        var counter = 1;
        while (await _context.EmailAccounts.AnyAsync(a => a.Name == newName && a.IsActive))
        {
            newName = $"{baseName} {counter++}";
        }

        var copy = new EmailAccount
        {
            Name = newName,
            Description = source.Description,
            SmtpHost = source.SmtpHost,
            SmtpPort = source.SmtpPort,
            SmtpUsername = source.SmtpUsername,
            SmtpPassword = source.SmtpPassword,
            FromEmail = source.FromEmail,
            FromName = source.FromName,
            EnableSsl = source.EnableSsl,
            IsDefault = false,
            DisplayOrder = source.DisplayOrder + 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.EmailAccounts.Add(copy);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email account copied: {SourceName} -> {CopyName}", source.Name, copy.Name);

        return CreatedAtAction(nameof(GetById), new { id = copy.Id }, new EmailAccountDto
        {
            Id = copy.Id,
            Name = copy.Name,
            Description = copy.Description,
            SmtpHost = copy.SmtpHost,
            SmtpPort = copy.SmtpPort,
            SmtpUsername = copy.SmtpUsername,
            FromEmail = copy.FromEmail,
            FromName = copy.FromName,
            EnableSsl = copy.EnableSsl,
            IsDefault = copy.IsDefault,
            DisplayOrder = copy.DisplayOrder,
            IsActive = copy.IsActive,
            CreatedAt = copy.CreatedAt,
            TemplateCount = 0
        });
    }

    // POST: api/admin/email-accounts/5/test
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestEmail(int id, [FromBody] TestEmailDto dto)
    {
        var account = await _context.EmailAccounts.FindAsync(id);
        if (account == null || !account.IsActive)
            return NotFound(new { message = "Email hesabi bulunamadi" });

        if (string.IsNullOrWhiteSpace(dto.ToEmail))
            return BadRequest(new { message = "Alici email adresi zorunludur" });

        try
        {
            using var client = new SmtpClient(account.SmtpHost, account.SmtpPort)
            {
                Credentials = new NetworkCredential(account.SmtpUsername, account.SmtpPassword),
                EnableSsl = account.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(account.FromEmail, account.FromName),
                Subject = "Erkan Tatil Plani - Test Email",
                Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2>Test Email Basarili!</h2>
                        <p>Bu email, <strong>{account.Name}</strong> hesabindan gonderildi.</p>
                        <hr/>
                        <p><small>Tarih: {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss} UTC</small></p>
                    </body>
                    </html>",
                IsBodyHtml = true
            };
            mailMessage.To.Add(dto.ToEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Test email sent from account {Name} to {ToEmail}", account.Name, dto.ToEmail);

            return Ok(new { message = $"Test emaili {dto.ToEmail} adresine basariyla gonderildi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email from account {Name} to {ToEmail}", account.Name, dto.ToEmail);
            return BadRequest(new { message = $"Email gonderilemedi: {ex.Message}" });
        }
    }
}
