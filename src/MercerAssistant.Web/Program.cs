using MercerAssistant.Web.Components;
using MercerAssistant.Core.Entities;
using MercerAssistant.Core.Enums;
using MercerAssistant.Core.Interfaces;
using MercerAssistant.Infrastructure.Data;
using MercerAssistant.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Blazor ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// --- Database ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --- ASP.NET Identity with Roles ---
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

builder.Services.AddCascadingAuthenticationState();

// Reduce security stamp validation interval so permission changes take effect quickly.
// When an admin updates a user's permissions, the user's cookie is invalidated within this window.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

// Register authorization policies for each feature permission
var authBuilder = builder.Services.AddAuthorizationBuilder();
foreach (var perm in AppPermission.All)
{
    authBuilder.AddPolicy(perm.Value, policy =>
        policy.RequireClaim(AppPermission.ClaimType, perm.Value));
}

// --- Application Services ---
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<IAIAssistantService, AIAssistantService>();
builder.Services.AddScoped<ICalendarProvider, GoogleCalendarService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBookingPageService, BookingPageService>();

var app = builder.Build();

// --- Seed default admin user ---
await SeedDataAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Identity UI Razor Pages (login, register, etc.)
app.MapRazorPages();

// Logout endpoint — Blazor Server interactive forms can't provide antiforgery tokens,
// so we use a dedicated endpoint with antiforgery disabled (auth-gated instead).
app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/");
}).RequireAuthorization().DisableAntiforgery();

app.Run();

static async Task SeedDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await db.Database.MigrateAsync();

    // Create roles if they don't exist
    string[] roles = ["Admin", "User"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Create default admin if not exists
    const string adminEmail = "admin@mercerassistant.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Jarrett Mercer",
            Role = MercerAssistant.Core.Enums.UserRole.Admin,
            TimeZone = "America/New_York",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");

            db.BookingPages.Add(new MercerAssistant.Core.Entities.BookingPage
            {
                Id = Guid.NewGuid(),
                OwnerId = adminUser.Id,
                Title = "Book a Meeting with Jarrett",
                Description = "Schedule a meeting at a time that works for you.",
                Slug = "jarrett",
                DefaultDurationMinutes = 30,
                MaxAdvanceDays = 60,
                MinNoticeHours = 2,
                BufferMinutes = 15,
                IsActive = true
            });

            await db.SaveChangesAsync();
        }
    }
    else
    {
        // Ensure existing admin user has the Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
