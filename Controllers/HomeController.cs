// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc; //Только один namespace вместо 7-8 в BookingController
                                //Не нужны: Data, Models, Authorization, Identity, EntityFrameworkCore, Security, Threading

namespace FMassage.Controllers
{
    public class HomeController : Controller //Наследуется от Controller как и BookingController, но не реализует сложную логику
    {
        private readonly ILogger<HomeController> _logger; // Только логгер для записи информации, не нужен доступ к БД
        
        public HomeController(ILogger<HomeController> logger) // Конструктор принимает только логгер, Dependency Injection автоматически передает логгер
        {
            _logger = logger; // Инициализация поля логгера
        }

        public IActionResult Index()  //Нет параметров - просто отображает страницу, нет async - не делает запросов к БД
        {
            return View(); // Отображает представление /Views/Home/Index.cshtml
        }
    }
}

/*
 * Назначение HomeController
HomeController обычно служит для:
Главной страницы сайта
Статических страниц (О нас, Контакты)
Обработки ошибок
Перенаправлений
Он не работает с бизнес-логикой (бронированиями, пользователями), поэтому ему не нужны:
Доступ к базе данных (ApplicationDbContext)
Управление пользователями (UserManager)
Сложная бизнес-логика

        // Не нужна работа с пользователями - нет UserManager
        //ILogger<HomeController> - унифицированный интерфейс для логирования
        //<HomeController> - указывает категорию логов(удобно для фильтрации)
        //readonly - можно установить только в конструкторе
*/