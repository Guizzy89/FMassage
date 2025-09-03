using FMassage.Data;
using FMassage.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку базы данных SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=FMassage.db"));

// Регистрация поддержки Asp.Net Core Identity
builder.Services.AddDefaultIdentity<User>(options =>
{
    // Настройки для совместимости с SQLite
    options.Stores.MaxLengthForKeys = 128;
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Включение подробных ошибок
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

// Стандартные middleware
app.UseStaticFiles();
app.UseRouting();

// Включаем процессы аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();



app.MapDefaultControllerRoute();
app.MapRazorPages();  // Важно для Identity!

app.Run();