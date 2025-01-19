using Microsoft.OpenApi.Models; // สำหรับ Swagger
using MeawMarket.Data; // Import DbContext
using Microsoft.EntityFrameworkCore; // สำหรับ SQL Server
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text; // สำหรับ Encoding Key

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(), // ตำแหน่งที่ถูกต้อง
    WebRootPath = @"C:\zoom\Meaw\MeawMarket\MeawMarket\wwwroot2",
    Args = args
});



// ตั้งค่า Service ต่างๆ
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

// เชื่อมต่อ DbContext กับ SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ระยะเวลาหมดอายุของ Session
    options.Cookie.HttpOnly = true; // เพื่อความปลอดภัย
    options.Cookie.IsEssential = true; // บังคับให้ใช้งาน Cookie
});


builder.Services.AddAuthorization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/user/login";
        options.LogoutPath = "/api/user/logout";
        options.AccessDeniedPath = "/user/accessdenied";
    });


var app = builder.Build();


app.Use(async (context, next) =>
{
    await next(); // ดำเนินการ Middleware ต่อไป

    if (context.Response.StatusCode == 401) // Unauthorized
    {
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest") // เช็คว่าเป็น AJAX หรือไม่
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Unauthorized. Please log in.",
                redirectUrl = "/api/user/login"
            }));
        }
        else
        {
            context.Response.Redirect("/api/user/login"); // 🔄 รีไดเร็กต์ไปหน้า Login ถ้าเป็น request ปกติ
        }
    }
});




app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run("http://localhost:5000");
