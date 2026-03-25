using Blazored.Toast;
using InventoryManagement.Data;
using InventoryManagement.Data.Models;
using InventoryManagement.Hubs;
using InventoryManagement.Services;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out var parsedPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{parsedPort}");
}

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var connectionString = ResolveConnectionString(builder.Configuration, builder.Environment);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

var authBuilder = builder.Services.AddAuthentication();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = facebookAppSecret;
    });
}

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddServerSideBlazor(options => options.DetailedErrors = true);
builder.Services.AddSignalR();
builder.Services.AddLocalization();
builder.Services.AddBlazoredToast();

builder.Services.Configure<SalesforceOptions>(builder.Configuration.GetSection("Salesforce"));
builder.Services.AddHttpClient<ISalesforceService, SalesforceService>();

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<ICustomIdGenerator, CustomIdGenerator>();
builder.Services.AddScoped<IFieldDefinitionService, FieldDefinitionService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IImageUploadService, LocalImageUploadService>();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await AdminSeeder.SeedAsync(scope.ServiceProvider);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    throw;
}

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures("en", "ru")
    .AddSupportedUICultures("en", "ru");

localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(localizationOptions);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<CommentHub>("/commentHub");
app.MapBlazorHub();
app.MapGet("/health", () => Results.Ok("OK"));
app.MapFallbackToPage("/_Host");

app.Run();

static string ResolveConnectionString(IConfiguration configuration, IWebHostEnvironment environment)
{
    var configured = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(configured) && !configured.Contains("localhost", StringComparison.OrdinalIgnoreCase))
    {
        return configured;
    }

    var databaseUrl = configuration["DATABASE_URL"] ?? Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        return ConvertDatabaseUrl(databaseUrl);
    }

    var pgHost = configuration["PGHOST"] ?? Environment.GetEnvironmentVariable("PGHOST");
    var pgPort = configuration["PGPORT"] ?? Environment.GetEnvironmentVariable("PGPORT");
    var pgDatabase = configuration["PGDATABASE"] ?? Environment.GetEnvironmentVariable("PGDATABASE");
    var pgUser = configuration["PGUSER"] ?? Environment.GetEnvironmentVariable("PGUSER");
    var pgPassword = configuration["PGPASSWORD"] ?? Environment.GetEnvironmentVariable("PGPASSWORD");

    if (!string.IsNullOrWhiteSpace(pgHost) &&
        !string.IsNullOrWhiteSpace(pgDatabase) &&
        !string.IsNullOrWhiteSpace(pgUser))
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = pgHost,
            Database = pgDatabase,
            Username = pgUser,
            Password = pgPassword ?? string.Empty,
            SslMode = SslMode.Require,
            TrustServerCertificate = true,
            Pooling = true
        };

        if (int.TryParse(pgPort, out var parsedPort))
        {
            builder.Port = parsedPort;
        }

        return builder.ConnectionString;
    }

    if (environment.IsDevelopment())
    {
        return "Host=localhost;Port=5432;Database=InventoryDb;Username=postgres;Password=password";
    }

    throw new InvalidOperationException("Database connection string is not configured.");
}

static string ConvertDatabaseUrl(string databaseUrl)
{
    if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
    {
        return databaseUrl;
    }

    if (!uri.Scheme.Equals("postgres", StringComparison.OrdinalIgnoreCase) &&
        !uri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
    {
        return databaseUrl;
    }

    var userInfo = uri.UserInfo.Split(':', 2);

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        SslMode = SslMode.Require,
        TrustServerCertificate = true,
        Pooling = true
    };

    return builder.ConnectionString;
}
