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
builder.Services.AddDefaultIdentity<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();


// �������� �������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

// ����������� middleware
app.UseStaticFiles();
app.UseRouting();

app.MapDefaultControllerRoute();

app.Run();