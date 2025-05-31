// Program.cs - THÊM ProductImageRepository
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình Identity với ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Cấu hình đường dẫn authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/AccessDenied/Index";
    options.ReturnUrlParameter = "returnUrl";
    options.SlidingExpiration = true;
});

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Đăng ký Repositories - THÊM ProductImageRepository
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();
builder.Services.AddScoped<IProductImageRepository, EFProductImageRepository>(); // ✅ THÊM DÒNG NÀY

var app = builder.Build();

// ✅ THÊM CODE TẠO ADMIN USER TỰ ĐỘNG
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedAdminUser(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating admin user");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware TRƯỚC Authentication và Authorization
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Thêm middleware để redirect các URL sai
app.Use(async (context, next) =>
{
    // Nếu truy cập /Identity/Home hoặc các path sai khác
    if (context.Request.Path.StartsWithSegments("/Identity/Home") ||
        context.Request.Path.StartsWithSegments("/Identity") && !context.Request.Path.StartsWithSegments("/Identity/Account"))
    {
        context.Response.Redirect("/");
        return;
    }
    await next();
});

// Routing configuration
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// MapRazorPages PHẢI có để Identity hoạt động
app.MapRazorPages();

app.Run();

// ✅ HÀM TẠO ADMIN USER TỰ ĐỘNG
static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Tạo các roles nếu chưa có
    string[] roles = { SD.Role_Admin, SD.Role_Customer, SD.Role_Company, SD.Role_Employee };

    foreach (string role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Tạo admin user nếu chưa có
    string adminEmail = "admin@shoppro.com";
    string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin ShopPro",
            Address = "123 Admin Street",
            Age = "30",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
            Console.WriteLine($"✅ Tạo admin user thành công:");
            Console.WriteLine($"   Email: {adminEmail}");
            Console.WriteLine($"   Password: {adminPassword}");
        }
        else
        {
            Console.WriteLine("❌ Lỗi tạo admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   - {error.Description}");
            }
        }
    }
    else
    {
        // Đảm bảo admin user có role Admin
        if (!await userManager.IsInRoleAsync(adminUser, SD.Role_Admin))
        {
            await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
        }
        Console.WriteLine($"✅ Admin user đã tồn tại: {adminEmail}");
    }
}