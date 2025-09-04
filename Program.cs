using FMassage.Data;
using FMassage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������� ������������ � �������������
builder.Services.AddControllersWithViews();

// ��������� ������� Razor Pages
builder.Services.AddRazorPages();

// ��������� ��������� ���� ������ SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=FMassage.db"));

// ����������� ��������� Asp.Net Core Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // ��������� ��� ������������� � SQLite
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

// ��������� ������� �����������
builder.Services.AddAuthorization();

var app = builder.Build();

// �������� ����� � ���������� ��������������
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    // ������� ����������� ����, ���� �� ���
    var roles = new[] { "Admin", "Manager", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ��������� ���� ������� ������������ ��� Admin
    var firstUser = await userManager.FindByEmailAsync("tarkhanovai@yandex.ru");
    if (firstUser != null && !await userManager.IsInRoleAsync(firstUser, "Admin"))
    {
        await userManager.AddToRoleAsync(firstUser, "Admin");
    }
}

// ����������� middleware
app.UseStaticFiles();
app.UseRouting();

// �������� �������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapRazorPages();  // ����� ��� Identity!

app.Run();