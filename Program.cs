using FMassage.Data; 
using FMassage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // Создает билдер приложения - объект для настройки сервисов и приложения

// Добавляем сервисы контроллеров и представлений
builder.Services.AddControllersWithViews(); // Регистрирует MVC паттерн - контроллеры + Razor views, Позволяет использовать контроллеры как BookingController, HomeController

// Добавляем сервисы Razor Pages
builder.Services.AddRazorPages(); // Регистрирует Razor Pages - для страниц без контроллеров

// Добавляем поддержку базы данных SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=FMassage.db"));
// Регистрирует контекст БД в DI контейнере
// UseSqlite() - указывает использовать SQLite базу данных
// builder.Configuration - читает настройки из appsettings.json
// ?? "Data Source=FMassage.db" - fallback если строка подключения не указана

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
        policy => policy.RequireRole("Admin")); // RequireRole("Admin") - требует чтобы пользователь был в роли "Admin"
});

var app = builder.Build(); // Строит приложение из зарегистрированных сервисов, После этого нельзя добавлять новые сервисы

// Создание ролей и назначение администратора
using (var scope = app.Services.CreateScope()) // создает область видимости для сервисов
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>(); //  получает зарегистрированные сервисы из DI контейнера
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
    var firstUser = await userManager.FindByEmailAsync("tarkhanovai@yandex.ru"); // ищет пользователя по email
    if (firstUser != null && !await userManager.IsInRoleAsync(firstUser, "Admin")) // добавляет пользователю роль
    {
        await userManager.AddToRoleAsync(firstUser, "Admin"); 
    }
}

// Стандартные middleware
app.UseStaticFiles(); // Обслуживает статические файлы (CSS, JS, изображения) из wwwroot
app.UseRouting(); // Включает маршрутизацию - определяет какой контроллер обрабатывает запрос

// Включаем процессы аутентификации и авторизации
app.UseAuthentication(); // проверяет кто пользователь (куки, токены)
app.UseAuthorization(); // проверяет что пользователь имеет права

app.MapDefaultControllerRoute(); // Настраивает стандартные маршруты для контроллеров
app.MapRazorPages();  // Важно для Identity!

app.Run();

/*
 * Как это работает на практике?
Запрос приходит на сервер
StaticFiles - проверяет не запрашивается ли статический файл
Routing - определяет куда направить запрос (контроллер или страница)
Authentication - проверяет кто пользователь (по кукам)
Authorization - проверяет есть ли у пользователя права
Controller/Page - обрабатывает запрос и возвращает ответ
Response - ответ отправляется обратно клиенту

Почему такая последовательность middleware важна?
Порядок критически важен:
UseStaticFiles() - до аутентификации, чтобы статические файлы были доступны всем
UseRouting() - должен быть до аутентификации и авторизации
UseAuthentication() - до авторизации (сначала узнать кто, потом что можно)
UseAuthorization() - после аутентификации и роутинга
Этот файл - мозг вашего приложения, который настраивает всю инфраструктуру ASP.NET Core!
*/