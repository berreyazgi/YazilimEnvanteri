using Microsoft.EntityFrameworkCore;
using YazilimEnvanteri.Data;
using Npgsql;
using YazilimEnvanteri.Services.Implementations;
using YazilimEnvanteri.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Context - kept registered for EF Core migrations/schema tooling only; runtime data
// access goes through Dapper (see Services/Implementations) rather than this context.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Pooled Npgsql data source for Dapper
builder.Services.AddSingleton(NpgsqlDataSource.Create(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' connection string is not configured.")));

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
