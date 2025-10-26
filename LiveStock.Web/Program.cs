using Microsoft.EntityFrameworkCore;
using LiveStock.Infrastructure.Data;
using LiveStock.Web.Service;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<adminService>();
builder.Services.AddScoped<sheepService>();
builder.Services.AddScoped<IStaffService, staffService>();

// Add Entity Framework with Azure SQL Server
builder.Services.AddDbContext<LiveStockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureConString")));

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// Serve images from the project Images folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/Images"
});

app.UseRouting();

app.UseAuthorization();

// Use Session
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
