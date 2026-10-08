using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using YazilimEnvanteri.Data;
using Npgsql;
using YazilimEnvanteri.Services.Implementations;
using YazilimEnvanteri.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Local: appsettings.Development.json (postgres from compose.staging.yml on localhost:5432).
// Docker: ConnectionStrings__DefaultConnection env var.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' connection string is not configured.");

// Database Context - kept registered for EF Core migrations/schema tooling only; runtime data
// access goes through Dapper (see Services/Implementations) rather than this context.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
