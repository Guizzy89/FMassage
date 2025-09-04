using FMassage.Data;
using FMassage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы контроллеров и представлений
builder.Services.AddControllersWithViews();

// Добавляем сервисы Razor Pages
builder.Services.AddRazorPages();

// Добавляем поддержку базы данных SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=FMassage.db"));

// Регистрация поддержки Asp.Net Core Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
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
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Добавляем сервисы авторизации с политикой для администраторов
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole",
        policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Создание ролей и назначение администратора
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    // Создаем стандартные роли, если их нет
    var roles = new[] { "Admin", "Manager", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Назначаем роль первому пользователю как Admin
    var firstUser = await userManager.FindByEmailAsync("tarkhanovai@yandex.ru");
    if (firstUser != null && !await userManager.IsInRoleAsync(firstUser, "Admin"))
    {
        await userManager.AddToRoleAsync(firstUser, "Admin");
    }
}

// Стандартные middleware
app.UseStaticFiles();
app.UseRouting();

// Включаем процессы аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapRazorPages();  // Важно для Identity!

app.Run();