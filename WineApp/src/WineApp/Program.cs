using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WineApp.Data;
using WineApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContextFactory<WineAppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=wineapp.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
})
.AddEntityFrameworkStores<WineAppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Register repositories
 builder.Services.AddSingleton<IWineProducerRepository, WineProducerRepository>();
builder.Services.AddSingleton<IWineRatingRepository, WineRatingRepository>();
builder.Services.AddSingleton<IWineRepository, WineRepository>();

var app = builder.Build();

// Apply EF Core migrations, seed data, roles and default admin on startup
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WineAppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { "Admin", "Judge", "Viewer", "WineProducer" })
    {
        if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
            roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
    }

    if (userManager.FindByEmailAsync("admin@wineapp.com").GetAwaiter().GetResult() is null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin@wineapp.com",
            Email = "admin@wineapp.com",
            DisplayName = "Administrator",
            EmailConfirmed = true
        };
        userManager.CreateAsync(admin, "Admin123!").GetAwaiter().GetResult();
        userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
