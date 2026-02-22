using System.Reflection;
using System.Threading.RateLimiting;
using ErkanTatilPlani.API.DependencyInjection;
using ErkanTatilPlani.API.Middleware;
using ErkanTatilPlani.API.Services;
using Microsoft.OpenApi.Models;
using ErkanTatilPlani.Core.Localization;
using ErkanTatilPlani.Core.Services;
using ErkanTatilPlani.Data.Context;
using CacheService = ErkanTatilPlani.API.Services.CacheService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Service + Factory Katman DI
builder.Services.AddInfrastructure();
builder.Services.AddEntityServices();
builder.Services.AddFactories();

// Localization Service
var localesPath = Path.Combine(builder.Environment.ContentRootPath, "Localization");
builder.Services.AddSingleton<ILocalizationService>(new LocalizationService(localesPath));

// Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Payment Service (Iyzico)
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection("Payment"));
builder.Services.AddScoped<IPaymentService, IyzicoPaymentService>();

// Cache Service
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// Response Caching
builder.Services.AddResponseCaching();

// HttpContextAccessor (log servisi icin)
builder.Services.AddHttpContextAccessor();

// App Log Service (veritabani loglama)
builder.Services.AddScoped<IAppLogService, AppLogService>();

// File Upload Service
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global limiter: 100 istek / dakika (IP bazli)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Auth endpoint'leri icin ozel limiter: 10 istek / dakika
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Sensitive endpoint'ler icin: 5 istek / dakika
    options.AddPolicy("sensitive", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Rate limit asildisinda response
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var response = new
        {
            type = "https://httpstatuses.com/429",
            title = "Cok fazla istek",
            status = 429,
            detail = "Istek limiti asildi. Lutfen bir sure bekleyip tekrar deneyin.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.TotalSeconds
                : 60
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, token);
    };
});

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApi();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Erkan Tatil Plani API",
        Description = "Tur kiralama ve rezervasyon sistemi API dokumantasyonu",
        Contact = new OpenApiContact
        {
            Name = "Erkan Tatil Plani",
            Email = "info@erkantatilplani.com"
        }
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Ornek: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML dokumantasyon
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migration on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandling(); // Global exception handler - ilk sirada olmali

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Erkan Tatil Plani API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Erkan Tatil Plani API";
        options.DefaultModelsExpandDepth(-1); // Modelleri gizle
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter(); // Rate limiting
app.UseStaticFiles();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
