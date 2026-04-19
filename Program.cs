using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PAS_Full_System.Data;
using PAS_Full_System.Models;
using Microsoft.AspNetCore.Identity;
using PAS_Full_System.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with Roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Configure Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// ==========================================
// DATA SEEDING: CREATE WEBMASTER ACCOUNT
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Create the WebMaster role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("WebMaster"))
    {
        await roleManager.CreateAsync(new IdentityRole("WebMaster"));
    }

    // 2. Check if the WebMaster user already exists
    string webMasterEmail = "webmaster@pas.com";
    var user = await userManager.FindByEmailAsync(webMasterEmail);
    
    // 3. If not, create the user with a secure, hashed password
    if (user == null)
    {
        var newWebMaster = new ApplicationUser
        {
            UserName = webMasterEmail,
            Email = webMasterEmail,
            FullName = "System Administrator",
            Department = "IT Operations",
            EmailConfirmed = true
        };

        // This hashes "AdminPassword123!" securely in your SQL database
        var result = await userManager.CreateAsync(newWebMaster, "AdminPassword123!");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newWebMaster, "WebMaster");
        }
    }
}
// ==========================================

// Configure pipeline
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Create roles and admin user on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Create roles
    string[] roles = { "Admin", "Supervisor", "Student", "ModuleLeader" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Create admin user
    var adminEmail = "admin@pas.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            Department = "Administration"
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Redirect authenticated users from home page to their dashboard
app.Use(async (context, next) =>
{
    await next();

    // If user is authenticated and on home page, redirect to their dashboard
    if (context.User.Identity.IsAuthenticated && context.Request.Path == "/")
    {
        if (context.User.IsInRole("Supervisor"))
            context.Response.Redirect("/Supervisor/Dashboard");
        else if (context.User.IsInRole("Student"))
            context.Response.Redirect("/Student/Dashboard");
        else if (context.User.IsInRole("Admin"))
            context.Response.Redirect("/Admin/Dashboard");
    }
});
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure Admin role exists
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Get admin user
    var adminUser = await userManager.FindByEmailAsync("admin@pas.com");
    if (adminUser != null)
    {
        // Add Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Admin role added to admin user");
        }
    }
}
app.Run();