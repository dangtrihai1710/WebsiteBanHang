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
.AddDefaultUI(); // Thêm dòng này

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
builder.Services.AddRazorPages(); // Quan trọng: Phải có dòng này

// Đăng ký Repositories
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

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