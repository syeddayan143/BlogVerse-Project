using BlogVerse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// DATABASE
// ----------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// ----------------------------------------------------
// IDENTITY
// ----------------------------------------------------

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // Password rules
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


// ----------------------------------------------------
// COOKIE CONFIGURATION
// ----------------------------------------------------

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});


// ----------------------------------------------------
// MVC
// ----------------------------------------------------

builder.Services.AddControllersWithViews();


// ----------------------------------------------------
// BUILD APP
// ----------------------------------------------------

var app = builder.Build();


// ----------------------------------------------------
// ERROR HANDLING
// ----------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// ----------------------------------------------------
// MIDDLEWARE
// ----------------------------------------------------

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// ----------------------------------------------------
// ROUTING
// ----------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();


// ----------------------------------------------------
// ADMIN ROLE + ADMIN USER SEED
// ----------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();


    // -----------------------------------------------
    // Create Admin Role
    // -----------------------------------------------

    const string adminRole = "Admin";

    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        var roleResult =
            await roleManager.CreateAsync(
                new IdentityRole(adminRole)
            );

        if (!roleResult.Succeeded)
        {
            throw new Exception(
                "Unable to create Admin role."
            );
        }
    }


    // -----------------------------------------------
    // Create Admin User
    // -----------------------------------------------

    const string adminEmail = "admin@blogverse.com";

    const string adminPassword = "Admin@123";

    var adminUser =
        await userManager.FindByEmailAsync(adminEmail);


    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Name = "BlogVerse Administrator",
            IsActive = true
        };

        var createResult =
            await userManager.CreateAsync(
                adminUser,
                adminPassword
            );

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                createResult.Errors.Select(e => e.Description)
            );

            throw new Exception(
                $"Unable to create Admin user: {errors}"
            );
        }
    }
    else
    {
        // Make sure existing admin account stays active
        if (!adminUser.IsActive)
        {
            adminUser.IsActive = true;

            await userManager.UpdateAsync(adminUser);
        }
    }


    // -----------------------------------------------
    // Add Admin Role
    // -----------------------------------------------

    if (!await userManager.IsInRoleAsync(
            adminUser,
            adminRole))
    {
        var roleResult =
            await userManager.AddToRoleAsync(
                adminUser,
                adminRole
            );

        if (!roleResult.Succeeded)
        {
            throw new Exception(
                "Unable to assign Admin role."
            );
        }
    }
}


app.Run();