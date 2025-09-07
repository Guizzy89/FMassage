using FMassage.Data; // Ваш контекст БД
using FMassage.Models; // Ваши модели (User, Booking)
using Microsoft.AspNetCore.Authorization; // Для авторизации
using Microsoft.AspNetCore.Identity;  // Для работы с пользователями
using Microsoft.AspNetCore.Mvc; // Для контроллеров и действий
using Microsoft.EntityFrameworkCore; // Для работы с БД
using System.Security.Claims; // Для информации о пользователе
using System.Threading.Tasks; // Для асинхронных методов

namespace FMassage.Controllers
{
    [Authorize] // Требуем авторизацию для всего контроллера,  означает, что ВСЕ методы контроллера требуют авторизации
    // Если пользователь не авторизован, его перенаправит на страницу входа, Исключения: методы с [AllowAnonymous]
    public class BookingController : Controller // имя контроллера (должно заканчиваться на "Controller")
    {
        private readonly ApplicationDbContext _context; // Контекст для взаимодействия с базой данных
        private readonly UserManager<User> _userManager; // Менеджер для работы с пользователями системы (например, получение текущего пользователя)
        // readonly - означает, что значения можно установить только в конструкторе
        /* Про метод ниже: Конструктор - вызывается при создании контроллера
        Параметры автоматически передаются системой Dependency Injection ASP.NET Core
        Это называется "внедрение зависимостей" - мы не создаем объекты сами, а получаем готовые
         */
        public BookingController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Booking - разный вид для админа и пользователей
        [AllowAnonymous] // Разрешаем доступ без авторизации, как и упоминалось выше, просмотр доступен всем гостям
        // Админ видит ВСЕ брони с информацией о клиентах
        public async Task<IActionResult> Index() //async Task<IActionResult> - асинхронный метод, возвращающий результат действия
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin")) // Проверяем, авторизован ли пользователь и является ли он админом
            {
                // Админ видит ВСЕ брони с информацией о клиентах
                var allBookings = await _context.Bookings // Получаем все брони из базы данных
                    .Include(b => b.User) // Загружает связанные данные о пользователе (жадная загрузка)
                    .ToListAsync(); // Асинхронно получает список из БД
                return View(allBookings); // Передаем данные в представление
            }
            else if (User.Identity.IsAuthenticated) // Авторизованный пользователь (не админ)
            {
                // Авторизованный пользователь видит только ДОСТУПНЫЕ слоты
                var availableSlots = await _context.Bookings // Получаем доступные слоты из базы данных
                    .Where(b => b.IsAvailable) // Фильтруем только доступные слоты
                    .ToListAsync(); // Асинхронно получает список из БД
                return View(availableSlots); // Передаем данные в представление
            }
            else
            {
                // Гость видит только ДОСТУПНЫЕ слоты
                var availableSlots = await _context.Bookings // Получаем доступные слоты из базы данных
                    .Where(b => b.IsAvailable) // Фильтруем только доступные слоты
                    .ToListAsync(); // Асинхронно получает список из БД
                return View(availableSlots); // Передаем данные в представление
            }
        }

        // GET: Booking/Create - только для админа
        [Authorize(Policy = "RequireAdminRole")] // Специальная политика для админов
        public IActionResult Create() //возвращает представление для создания новой брони
        {
            return View(); // Возвращаем пустое представление для создания новой брони
        }

        // POST: Booking/Create - только для админа
        [HttpPost] // Метод обрабатывает только POST-запросы (от форм), Параметры автоматически связываются с данными из формы
        [Authorize(Policy = "RequireAdminRole")] // Cпециальная политика для админов
        public async Task<IActionResult> Create(DateTime slotDate, int durationInMinutes) // Параметры из формы
        {
            var booking = new Booking // Создаем новую бронь
            {
                SlotDate = slotDate, // Дата и время слота
                Duration = TimeSpan.FromMinutes(durationInMinutes), // Продолжительность
                IsAvailable = true // По умолчанию слот доступен
            };

            _context.Bookings.Add(booking); // Добавляет новую запись в контекст
            await _context.SaveChangesAsync(); // Асинхронно сохраняет изменения в базе данных
            return RedirectToAction(nameof(Index)); // Перенаправляет на действие Index (список броней)
        }

        // GET: Booking/Edit/id - только для админа
        [Authorize(Policy = "RequireAdminRole")] // Cпециальная политика для админов
        public async Task<IActionResult> Edit(int id) // Получаем id брони для редактирования
        {
            var booking = await _context.Bookings.FindAsync(id); // Ищет запись по первичному ключу
            if (booking == null) // Если запись не найдена
            {
                return NotFound(); // Возвращает HTTP 404 если запись не найдена
            }
            return View(booking); // Передаем данные в представление для редактирования
        }

        // POST: Booking/Edit/id - только для админа
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")] // Cпециальная политика для админов
        public async Task<IActionResult> Edit(int id, DateTime slotDate, int durationInMinutes) // Параметры из формы
        {
            var booking = await _context.Bookings.FindAsync(id); // Ищет запись по первичному ключу
            if (booking == null) // Если запись не найдена
            {
                return NotFound(); // Возвращает HTTP 404 если запись не найдена
            }

            booking.SlotDate = slotDate; // Обновляем дату и время слота
            booking.Duration = TimeSpan.FromMinutes(durationInMinutes); // Обновляем продолжительность

            await _context.SaveChangesAsync(); // Асинхронно сохраняем изменения в базе данных
            return RedirectToAction(nameof(Index)); // Перенаправляем на действие Index (список броней)
        }

        // GET: Booking/Delete/id - только для админа
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Delete(int id) // Получаем id брони для удаления
        {
            var booking = await _context.Bookings.FindAsync(id); // Ищет запись по первичному ключу
            if (booking == null) // Если запись не найдена
            {
                return NotFound(); // Возвращает HTTP 404 если запись не найдена
            }
            return View(booking); // Передаем данные в представление для подтверждения удаления
        }

        // POST: Booking/Delete/id - только для админа
        [HttpPost, ActionName("Delete")] // Позволяет использовать другое имя метода, но тот же URL
        // Это нужно потому что нельзя иметь два метода с одинаковыми параметрами
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteConfirmed(int id) // Получаем id брони для удаления
        {
            var booking = await _context.Bookings.FindAsync(id); // Ищет запись по первичному ключу
            if (booking != null) // Если запись найдена
            {
                _context.Bookings.Remove(booking); // Удаляет запись из контекста
                await _context.SaveChangesAsync(); // Асинхронно сохраняет изменения в базе данных
            }
            return RedirectToAction(nameof(Index)); // Перенаправляем на действие Index (список броней)
        }

        // GET: Booking/Book/id - бронирование слота пользователем
        public async Task<IActionResult> Book(int id) // Получаем id слота для бронирования
        {
            var booking = await _context.Bookings.FindAsync(id); // Ищет запись по первичному ключу
            if (booking == null || !booking.IsAvailable) // Если запись не найдена или слот уже забронирован
            {
                return NotFound(); // Возвращает HTTP 404 если запись не найдена или слот недоступен
            }

            return View(booking); // Передаем данные в представление для подтверждения бронирования
        } // Проверяет, доступен ли слот для бронирования

        // POST: Booking/Book/id - обработка бронирования
        [HttpPost] // Метод обрабатывает только POST-запросы (от форм)
        public async Task<IActionResult> Book(int id, string clientName, string phoneNumber, string comment) // Параметры из формы
        {
            var booking = await _context.Bookings.FindAsync(id); // Ищет запись по первичному ключу
            if (booking == null || !booking.IsAvailable) // Если запись не найдена или слот уже забронирован
            {
                return NotFound(); // Возвращает HTTP 404 если запись не найдена или слот недоступен
            }

            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User); //  Получает объект пользователя из БД
            // Если пользователь не найден (что маловероятно, так как метод защищен [Authorize])
            // User - текущий авторизованный пользователь (из cookie)
            // Заполняем данные брони
            booking.IsAvailable = false; // Слот теперь недоступен
            booking.ClientName = clientName; // Имя клиента
            booking.PhoneNumber = phoneNumber; // Телефон клиента
            booking.Comment = comment; // Комментарий клиента
            booking.UserId = user.Id; // Связываем бронь с пользователем

            await _context.SaveChangesAsync(); // Асинхронно сохраняем изменения в базе данных

            return RedirectToAction(nameof(MyBookings)); // Перенаправляем на страницу с моими бронями
        } //Обновляет свойства брони и сохраняет изменения

        // GET: Booking/MyBookings - просмотр своих броней
        public async Task<IActionResult> MyBookings() // Метод для просмотра своих броней
        {
            var userId = _userManager.GetUserId(User); // Получаем ID текущего пользователя
            var userBookings = await _context.Bookings // Получаем брони текущего пользователя из базы данных
                .Where(b => b.UserId == userId) // Фильтруем по ID пользователя
                .ToListAsync(); // Асинхронно получает список из БД

            return View(userBookings); // Передаем данные в представление
        }
    }
}

/* 1. Маршрутизация
GET: Booking/Index - отображает список
GET: Booking/Create - форма создания
POST: Booking/Create - обработка формы
GET: Booking/Edit/5 - форма редактирования
POST: Booking/Edit/5 - обработка редактирования

2. Возвращаемые типы
View() - возвращает HTML-страницу
RedirectToAction() - перенаправляет на другой метод
NotFound() - возвращает 404 ошибку

3. Асинхронность
async/await - не блокируют поток выполнения
Task<IActionResult> - асинхронный возвращаемый тип

4. Безопасность
[Authorize] - защита от неавторизованного доступа
[Authorize(Policy = "...")] - дополнительная проверка прав
Проверки в коде (IsAvailable, null проверки)
Этот контроллер реализует полный CRUD (Create, Read, Update, Delete) для бронирований с разными уровнями доступа для пользователей и администраторов.
 */