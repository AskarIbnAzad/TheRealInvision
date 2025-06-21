using Microsoft.EntityFrameworkCore;
using System;
using TheRealInvision.Interfaces;
using TheRealInvision.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repositories for Dependency Injection
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();

// Configure session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout duration
    options.Cookie.HttpOnly = true;               // Ensures session cookie is not accessible via client-side scripts
    options.Cookie.IsEssential = true;            // Required for GDPR compliance
});

// Add HttpContextAccessor for accessing session and HTTP context globally
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline

// Enable static files (e.g., CSS, JavaScript, images)
app.UseStaticFiles();

// Enable session middleware
app.UseSession();

// Middleware for authentication/session validation
app.Use(async (context, next) =>
{
    // Redirect users to login if session is empty and they're not accessing the login page
    if (string.IsNullOrEmpty(context.Session.GetString("UserEmail")) &&
        !context.Request.Path.StartsWithSegments("/Account/Login"))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    await next();
});

// Configure error handling for production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Use custom error handler
    app.UseHsts();                         // Enable HSTS for HTTPS
}

// Configure HTTPS redirection and security
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization(); // Authorization middleware

// Map static assets (optional if implemented in your project)
app.MapStaticAssets();

// Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Project}/{action=Index}/{id?}")
    .WithStaticAssets(); // Optional chaining for static assets routing

// Run the application
app.Run();
