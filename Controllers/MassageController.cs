using FMassage.Data; // Для доступа к ApplicationDbContext
using FMassage.Models; // Для работы с моделью Massage
using Microsoft.AspNetCore.Authorization; // Для атрибута авторизации
using Microsoft.AspNetCore.Mvc; // Для контроллеров и действий
using Microsoft.EntityFrameworkCore; // Для работы с Entity Framework Core
using System.Threading.Tasks; // Для асинхронного программирования

namespace FMassage.Controllers
{
    // Объявление класса контроллера для управления массажными услугами
    // Наследуется от Controller - базового класса ASP.NET Core для контроллеров
    public class MassageController : Controller
    {
        // Приватное поле для работы с базой данных
        // readonly означает, что значение можно установить только в конструкторе
        private readonly ApplicationDbContext _context;

        // Конструктор контроллера с внедрением зависимости (Dependency Injection)
        // ApplicationDbContext автоматически передается системой DI ASP.NET Core
        public MassageController(ApplicationDbContext context)
        {
            // Сохраняем переданный контекст базы данных в поле класса
            _context = context;
        }

        // Метод для отображения списка всех массажных услуг
        // async указывает на асинхронный метод
        // Task<IActionResult> - возвращает задачу, которая завершится результатом действия
        // Индексная страница для вывода списка услуг (GET-запрос) - доступно всем
        public async Task<IActionResult> Index()
        {
            // Асинхронно получаем все записи из таблицы Massages и преобразуем в список
            // ToListAsync() не блокирует поток выполнения во время ожидания данных из БД
            var services = await _context.Massages.ToListAsync();
            return View(services); // Возвращаем представление с передачей списка услуг в качестве модели
        }

        // Открытие формы для создания новой услуги (GET-запрос) - только для Admin
        // GET-версия метода создания новой услуги
        [HttpGet]  // [HttpGet] - явно указывает, что метод обрабатывает GET-запросы
        [Authorize(Policy = "RequireAdminRole")] // [Authorize(Policy = "RequireAdminRole")] - ограничивает доступ только для админов
        public IActionResult Create()
        {
            return View();
            // Возвращает представление с формой для создания новой услуги
            // Пустая форма без предварительных данных
        }

        // Сохранение новой услуги (POST-запрос) - только для Admin
        [HttpPost] // [HttpPost] - обрабатывает только POST-запросы (данные из формы)
        [Authorize(Policy = "RequireAdminRole")] // [Authorize(Policy = "RequireAdminRole")] - только для админов
        public async Task<IActionResult> Create(Massage massage) // Параметр Massage massage - модель связывается с данными из формы автоматически
        {
            if (ModelState.IsValid) // Проверка валидности модели на основе Data Annotations в классе Massage
            {
                _context.Massages.Add(massage); // Добавляем новую услугу в контекст базы данных
                await _context.SaveChangesAsync(); // Асинхронно сохраняем изменения в базе данных
                return RedirectToAction(nameof(Index)); // После успешного сохранения перенаправляем на страницу со списком услуг
            }
            return View(massage); // Если модель не валидна, возвращаем форму с введенными данными для исправления ошибок
        }

        // Редактирование существующей услуги (GET-запрос) - только для Admin
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")] // Только для админов
        public async Task<IActionResult> Edit(int id) 
        {
            var service = await _context.Massages.FindAsync(id); // Ищем услугу по идентификатору в БД, FindAsync() эффективно ищет по первичному ключу
            if (service == null) // Если услуга не найдена, возвращаем 404
            {
                return NotFound(); // Возвращаем статус 404, если услуга с таким ID не найдена
            }
            return View(service); // Возвращаем представление с формой для редактирования, заполняя её текущими данными услуги
        }

        // Обработка изменений (POST-запрос) - только для Admin
        [HttpPost] // Обрабатывает POST-запросы 
        [Authorize(Policy = "RequireAdminRole")] // Только для админов
        public async Task<IActionResult> Edit(int id, Massage massage) // Параметры: id услуги и обновленная модель Massage
        {
            if (id != massage.Id) // Проверяем, что ID из URL совпадает с ID в модели, защита от подмены идентификатора
            {
                return NotFound(); // Возвращаем 404, если ID не совпадают
            }

            if (ModelState.IsValid) // Проверяем валидность модели
            {
                try
                {
                    _context.Update(massage); // Обновляем услугу в контексте базы данных
                    await _context.SaveChangesAsync(); // Асинхронно сохраняем изменения в базе данных
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                    // Обработка исключения конкурентного доступа
                    // (если запись была изменена другим пользователем)
                    // В данном случае просто пробрасываем исключение дальше
                }
                return RedirectToAction(nameof(Index)); // После успешного обновления перенаправляем на страницу со списком услуг
            }
            return View(massage); // Если модель не валидна, возвращаем форму с введенными данными для исправления ошибок
        }

        // Форма для удаления услуги (GET-запрос) - только для Admin
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")] // Только для админов
        public async Task<IActionResult> Delete(int id) // Параметр id - идентификатор услуги для удаления
        {
            var service = await _context.Massages.FindAsync(id); // Ищем услугу для удаления по идентификатору в БД
            if (service == null)
            {
                return NotFound(); // Возвращаем 404, если услуга с таким ID не найдена
            }
            return View(service); // Возвращаем представление с подтверждением удаления, показывая детали услуги
        }

        // Обработка удаления (POST-запрос) - только для Admin
        [HttpPost, ActionName("Delete")] // Обрабатывает POST-запросы, ActionName("Delete") позволяет использовать одно имя действия для GET и POST
        [Authorize(Policy = "RequireAdminRole")] // Только для админов
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Massages.FindAsync(id); // Ищем услугу по идентификатору в БД
            if (service != null)
            {
                _context.Massages.Remove(service); // Удаляем услугу из контекста базы данных
                await _context.SaveChangesAsync(); // Асинхронно сохраняем изменения в базе данных
            }
            return RedirectToAction(nameof(Index)); // После удаления перенаправляем на страницу со списком услуг
        }
    }
}
/*
 * Ключевые особенности этого контроллера:
1. CRUD операции
Create - создание новых услуг
Read - просмотр списка услуг (Index)
Update - редактирование существующих услуг
Delete - удаление услуг

2. Разделение GET/POST методов
GET методы показывают формы
POST методы обрабатывают данные форм

3. Валидация модели
ModelState.IsValid проверяет данные на основе атрибутов валидации в модели

4. Обработка ошибок
Проверка существования записей (FindAsync + null проверка)
Обработка исключений конкурентности

5. Безопасность
Авторизация только для админов на изменяющих операциях
Открытый доступ только для просмотра (Index)

6. Асинхронность
Все методы, работающие с БД, асинхронные
Не блокируют потоки выполнения во время ожидания БД
Это классический пример контроллера с полным набором CRUD операций, правильно использующий паттерны ASP.NET Core.
*/