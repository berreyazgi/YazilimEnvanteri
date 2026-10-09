using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Data;
using YazilimEnvanteri.Models.Identity;
using Npgsql;
using YazilimEnvanteri.Services.Implementations;
using YazilimEnvanteri.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Every unsafe-method request (POST forms and the JSON fetch() calls alike) must carry the
// antiforgery token; the layout exposes it to JS (see AppConfig.antiforgeryHeaders).
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Local: appsettings.Development.json (postgres from compose.staging.yml on localhost:5432).
// Docker: ConnectionStrings__DefaultConnection env var.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' connection string is not configured.");

// Database Context - kept registered for EF Core migrations/schema tooling only; runtime data
// access goes through Dapper (see Services/Implementations) rather than this context.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ASP.NET Core Identity on the same database (AspNetUsers/AspNetRoles/...), cookie-based sign-in.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;

    // Page navigations get the Login/AccessDenied redirect; fetch() calls get a plain 401/403
    // instead of a redirect that would be followed and come back as a misleading 200 HTML page.
    options.Events.OnRedirectToLogin = ctx => RedirectOrStatus(ctx, StatusCodes.Status401Unauthorized);
    options.Events.OnRedirectToAccessDenied = ctx => RedirectOrStatus(ctx, StatusCodes.Status403Forbidden);
});

// Re-reads the user's roles from the database every minute, so a role change made in
// Kullanıcı Yönetimi reaches already signed-in users quickly (default is 30 minutes).
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(1));

builder.Services.AddAuthorization(options => options.AddAppPolicies());

// Pooled Npgsql data source for Dapper
var dataSource = NpgsqlDataSource.Create(connectionString);
builder.Services.AddSingleton(dataSource);

// /health: 200 only when the app is up AND postgres answers.
builder.Services.AddHealthChecks().AddAsyncCheck("postgres", async ct =>
{
    try
    {
        await using var cmd = dataSource.CreateCommand("SELECT 1");
        await cmd.ExecuteScalarAsync(ct);
        return HealthCheckResult.Healthy();
    }
    catch (Exception ex)
    {
        return HealthCheckResult.Unhealthy(exception: ex);
    }
});

// Per-entity services (Dapper-backed, no repository layer)
builder.Services.AddScoped<IProjeService, ProjeService>();
builder.Services.AddScoped<IProjeExportService, ProjeExportService>();
builder.Services.AddScoped<BirimService>();
builder.Services.AddScoped<YazilimUzmaniService>();
builder.Services.AddScoped<TeknolojiService>();

var app = builder.Build();

await IdentitySeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Docker healthcheck - must answer without a session.
app.MapHealthChecks("/health").AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static Task RedirectOrStatus(RedirectContext<CookieAuthenticationOptions> ctx, int statusCode)
{
    if (ctx.Request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase))
        ctx.Response.Redirect(ctx.RedirectUri);
    else
        ctx.Response.StatusCode = statusCode;
    return Task.CompletedTask;
}
