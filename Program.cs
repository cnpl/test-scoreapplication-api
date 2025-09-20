using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScoreTracker.Api.Data;
using ScoreTracker.Api.Models.Entities;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// === 1. CONFIGURE SERVICES ===

// Add services to the dependency injection container.

// --- CORS Policy ---
// Allows the Angular WebApp to make requests to this API.
var webAppUrl = builder.Configuration.GetValue<string>("Cors:WebAppUrl") 
    ?? throw new InvalidOperationException("Cors:WebAppUrl not configured.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(webAppUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Important for cookies/sessions
    });
});

// --- Database Context ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- ASP.NET Core Identity ---
// Configures user and role management.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- Authentication ---
// We use a cookie-based scheme for session management.
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies(); 

builder.Services.AddAuthorization();

// --- Data Protection (CRITICAL FOR SHARED SESSION) ---
// Configures keys to be stored in a shared location (Azure Blob Storage).
// Both Api and WebApp projects MUST point to the same location.
var dataProtectionBlobUri = new Uri(builder.Configuration.GetValue<string>("AzureDataProtection:BlobStorageUri")
    ?? throw new InvalidOperationException("AzureDataProtection:BlobStorageUri not configured."));

builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(dataProtectionBlobUri)
    // CRITICAL: This application name is used to isolate cookies. 
    // It MUST be the same in both the Api and WebApp projects.
    .SetApplicationName("ScoreTrackerShared");

// --- Distributed Session Cache using Azure Redis ---
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' not found.");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "ScoreTracker_"; // Optional prefix for keys
});

// --- Session Configuration (CRITICAL FOR SHARED SESSION) ---
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ScoreTracker.Session";
    // CRITICAL: The domain must be set to the parent domain to allow
    // the cookie to be shared between api.yourdomain.com and app.yourdomain.com.
    // options.Cookie.Domain = ".yourdomain.com"; // UNCOMMENT AND SET IN PRODUCTION
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === 2. CONFIGURE HTTP REQUEST PIPELINE (MIDDLEWARE) ===

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

// The order of middleware is VERY important.
app.UseCors("AllowWebApp");

app.UseAuthentication(); // 1. Who is the user? (Identifies the user from the cookie)
app.UseAuthorization();  // 2. Are they allowed to do this? (Checks roles/claims)

app.UseSession();        // 3. Load the session data from Redis for this user.
                         // MUST be after Authentication and Authorization.

app.MapControllers();

app.Run();
