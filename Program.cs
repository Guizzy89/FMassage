using FMassage.Data;
using FMassage.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� ���� ������ SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=FMassage.db"));

// ����������� ��������� Asp.Net Core Identity
builder.Services.AddDefaultIdentity<User>(options =>
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
.AddEntityFrameworkStores<ApplicationDbContext>();

// ��������� ��������� ������
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

// ����������� middleware
app.UseStaticFiles();
app.UseRouting();

// �������� �������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();



app.MapDefaultControllerRoute();
app.MapRazorPages();  // ����� ��� Identity!

app.Run();