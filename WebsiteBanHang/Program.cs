using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session - THÊM MỚI
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout 30 phút
    options.Cookie.HttpOnly = true; // Bảo mật cookie
    options.Cookie.IsEssential = true; // Cookie cần thiết
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
.AddDefaultTokenProviders();

// Cấu hình đường dẫn khi bị từ chối truy cập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/AccessDenied/Index";
});

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Đăng ký Repositories
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// QUAN TRỌNG: Thêm Session middleware - TRƯỚC Authentication và Authorization
app.UseSession();

// QUAN TRỌNG: Thứ tự Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

// QUAN TRỌNG: Cấu hình routing cho Admin Area TRƯỚC routing mặc định
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();