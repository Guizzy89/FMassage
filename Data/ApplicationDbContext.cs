using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // IdentityDbContext предоставляет контекст базы данных, интегрированный с ASP.NET Core Identity
using Microsoft.EntityFrameworkCore; // Для DbContext и DbSet
using FMassage.Models; // Для моделей User, Massage, Booking

namespace FMassage.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> // Наследуемся от IdentityDbContext с пользовательской моделью User
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) // Конструктор принимает параметры конфигурации контекста
            : base(options) // Передаем параметры базовому классу
        {
            // Пустой конструктор - вся настройка происходит через options
            // которые передаются через Dependency Injection в Program.cs
        }

        public DbSet<Massage> Massages { get; set; } // DbSet для таблицы массажных услуг,  Entity Framework автоматически создаст таблицу "Massages" в БД
        public DbSet<Booking> Bookings { get; set; } // DbSet для таблицы бронирований, Entity Framework создаст таблицу "Bookings" в БД

        protected override void OnModelCreating(ModelBuilder builder) // Метод для настройки модели данных, вызывается автоматически Entity Framework при создании модели
        {
            // Вызов базовой реализации метода из IdentityDbContext
            // Это ОЧЕНЬ важно - базовая реализация настраивает все таблицы
            // для Identity: Users, Roles, UserRoles, UserClaims, UserLogins и т.д.
            // Без этого вызова система аутентификации не будет работать правильно
            base.OnModelCreating(builder);

            // Настраиваем связь между Booking и User
            // Начинаем настройку конкретной сущности (таблицы) Booking
            // Fluent API - более мощный способ конфигурации compared to Data Annotations
            builder.Entity<Booking>() 
                .HasOne(b => b.User) // Указываем, что у одного пользователя может быть много бронирований,  u => u.Bookings - навигационное свойство в классе User (коллекция)
                .WithMany(u => u.Bookings) // Указываем внешний ключ в таблице Bookings, который ссылается на пользователя
                .HasForeignKey(b => b.UserId) // Внешний ключ в таблице Bookings, который ссылается на пользователя
                .OnDelete(DeleteBehavior.Restrict); // При удалении User связанные Booking не будут удалены автоматически
        }
    }
}

/*
 * Что происходит "под капотом":
1. IdentityDbContext<User> автоматически создает:
AspNetUsers - таблица пользователей (расширяет стандартную таблицу)
AspNetRoles - таблица ролей
AspNetUserRoles - таблица связи пользователей и ролей (many-to-many)
AspNetUserClaims - claims пользователей
AspNetUserLogins - внешние логины (Google, Facebook и т.д.)
AspNetUserTokens - токены пользователей

2. DbContextOptions содержит:
// Пример настройки в Program.cs:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

3. Fluent API vs Data Annotations:
Data Annotations - атрибуты в моделях ([Required], [MaxLength])
Fluent API - более мощный, не "засоряет" модели, централизованная конфигурация

4. Поведения удаления (DeleteBehavior):
Restrict - "Запретить удаление если есть зависимости" (безопасно)
Cascade - "Удалить все связанные записи" (опасно, может привести к потере данных)
SetNull - "Установить NULL в внешнем ключе" (требует nullable поле)

5. Миграции:
На основе этого контекста Entity Framework создает миграции:
bash
dotnet ef migrations add InitialCreate
dotnet ef database update
Этот файл является центральным узлом всей системы данных приложения, объединяя Identity систему с вашими бизнес-моделями и обеспечивая правильные отношения между таблицами.
*/