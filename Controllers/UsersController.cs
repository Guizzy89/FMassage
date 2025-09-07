using FMassage.Models; // Для работы с моделью User
using FMassage.ViewModels; // Для работы с моделями представлений (например, RegisterViewModel, LoginViewModel)
using Microsoft.AspNetCore.Authorization; // Для атрибута [Authorize]
using Microsoft.AspNetCore.Identity; // Для работы с UserManager и SignInManager
using Microsoft.AspNetCore.Mvc; // Для контроллеров и действий
using System.Diagnostics; // Для отладки (Debug.WriteLine)

namespace FMassage.Controllers
{
    public class UsersController : Controller // Контроллер для управления пользователями: регистрация, авторизация, профиль
    {
        private readonly UserManager<User> _userManager; // Менеджер для работы с пользователями (создание, поиск, управление паролями)
        private readonly SignInManager<User> _signInManager; // Менеджер для работы аутентификацией (вход, выход, проверка)

        public UsersController(UserManager<User> userManager, SignInManager<User> signInManager) // Конструктор с внедрением зависимостей
        {
            _userManager = userManager; // Инициализация менеджера пользователей
            _signInManager = signInManager; // Инициализация менеджера аутентификации
        }

        #region Регистрация пользователя // Region группирует связанный код для лучшей читаемости

        [HttpGet]   // GET: /Users/Register - отображение формы регистрации
        public IActionResult Register()
        {
            return View(new RegisterViewModel()); // Возвращаем представление с новой пустой моделью RegisterViewModel
        }

        [HttpPost] // POST: /Users/Register - обработка данных формы регистрации
        public async Task<IActionResult> Register(RegisterViewModel model) // Асинхронный метод для регистрации пользователя
        {
            if (ModelState.IsValid) // Проверяем, что модель валидна (все обязательные поля заполнены корректно)
            {
                try
                {
                    var user = new User { UserName = model.Email, Email = model.Email }; // Создаем нового пользователя с email в качестве имени пользователя
                    var result = await _userManager.CreateAsync(user, model.Password); // Пытаемся создать пользователя с указанным паролем

                    if (result.Succeeded) // Если создание прошло успешно
                    {
                        await _signInManager.SignInAsync(user, false); // Вход после регистрации, false - означает, что сессия не сохраняется после закрытия браузера
                        return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу
                    }

                    foreach (var error in result.Errors) // Если были ошибки при создании пользователя, добавляем их в ModelState для отображения
                    {
                        ModelState.AddModelError("", error.Description); // Добавляем ошибку без привязки к конкретному полю
                    }
                }
                catch (Exception ex)  // Логируем ошибку в отладочный вывод (видно в Visual Studio Output window)
                {
                    // Сообщаем о возникшей ошибке
                    Debug.WriteLine(ex.Message + "\n" + ex.StackTrace); // Выводим сообщение и стек вызовов в отладочный вывод
                    return View(model); // Возвращаем представление с моделью, чтобы пользователь мог повторить попытку
                }
            }

            return View(model); // Если модель не валидна, возвращаем форму с данными
        }

        #endregion

        #region Авторизация пользователя

        [HttpGet]  // GET: /Users/Login - отображение формы входа
        public IActionResult Login(string? returnUrl = null) // Проверка безопасности: returnUrl должен быть локальным URL
        {
            // Проверяем, что returnUrl локальный URL для безопасности
            if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl)) // Url.IsLocalUrl предотвращает открытые перенаправления
            {
                returnUrl = null; // Если URL не локальный, сбрасываем его
            }

            ViewBag.ReturnUrl = returnUrl; // Передаем returnUrl в представление через ViewBag
            return View(); // Возвращаем представление с формой входа
        }

        [HttpPost] // POST: /Users/Login - обработка  входа
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null) // Асинхронный метод для входа пользователя
        {
            // Проверяем returnUrl на безопасность
            if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = null;
            }

            if (ModelState.IsValid) // Проверяем валидность модели
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, lockoutOnFailure: false); // Пытаемся войти пользователя с указанными данными

                if (result.Succeeded) // Если вход успешен
                {
                    return RedirectToLocal(returnUrl); // Перенаправляем на returnUrl или на главную страницу
                }

                ModelState.AddModelError("", "Неверный адрес электронной почты или пароль."); // Если вход не удался, добавляем ошибку в ModelState
            }

            ViewBag.ReturnUrl = returnUrl; // Сохраняем returnUrl для повторного отображения формы
            return View(model); // Возвращаем представление с моделью для повторного ввода
        }

        private IActionResult RedirectToLocal(string? returnUrl) // Вспомогательный метод для безопасного перенаправления
        {
            if (Url.IsLocalUrl(returnUrl)) // Проверяем, что returnUrl локальный
            {
                return Redirect(returnUrl); // Безопасное перенаправление на локальный URL
            }
            else
            {
                return RedirectToAction("Index", "Home"); // Если URL не локальный, перенаправляем на главную страницу
            }
        }

        #endregion

        #region Выход из аккаунта

        [Authorize] // Только авторизованные пользователи могут выйти из системы
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // Асинхронный выход пользователя из системы
            return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу после выхода
        }

        #endregion

        #region Изменение пароля 

        [Authorize]   
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]  // POST: /Users/ChangePassword - обработка изменения пароля
        [Authorize]  // Только авторизованные пользователи могут изменить пароль
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User); //Получаем текущего пользователя из контекста HTTP
                if (user == null) // Если пользователь не найден, возвращаем вызов повторной аутентификации
                {
                    return Challenge();  // Возвращаем challenge (перенаправление на страницу входа)
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword); // Пытаемся изменить пароль
                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user); // Обновляем сессию пользователя после изменения пароля
                    TempData["Success"] = "Ваш пароль успешно изменён."; // Сообщение об успешном изменении пароля
                    return RedirectToAction("Profile"); // Перенаправляем на страницу профиля
                }

                foreach (var error in changePasswordResult.Errors) // Если были ошибки при изменении пароля, добавляем их в ModelState для отображения
                {
                    ModelState.AddModelError("", error.Description); // Добавляем ошибку без привязки к конкретному полю
                }
            }

            return View(model); // Если модель не валидна или были ошибки, возвращаем форму с введенными данными для исправления ошибок
        }

        #endregion

        #region Просмотр профиля текущего пользователя
        // GET: /Users/Profile - страница профиля пользователя
        [Authorize]
        public async Task<IActionResult> Profile() 
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);  // Получаем текущего авторизованного пользователя
            if (user == null) 
            {
                return NotFound($"Пользователь с таким ID не найден.");     // Возвращаем 404, если пользователь не найден (что маловероятно, так как пользователь авторизован)
            }

            return View(user);
        }

        #endregion
    }
}
/*
 * Ключевые особенности этого контроллера:
1. Работа с Identity System
Использует встроенную систему аутентификации ASP.NET Core Identity
UserManager - для управления пользователями и паролями
SignInManager - для управления сессиями и аутентификацией

2. Безопасность
Валидация моделей - проверка входных данных
Проверка returnUrl - защита от open redirect атак
Атрибут [Authorize] - защита敏感чных действий
Хэширование паролей - автоматическое в UserManager

3. Поток работы
Регистрация:
Показ формы → 2. Валидация → 3. Создание пользователя → 4. Автовход → 5. Перенаправление
Вход:
Показ формы → 2. Валидация → 3. Проверка учетных данных → 4. Создание сессии → 5. Перенаправление
Смена пароля:
Проверка авторизации → 2. Валидация → 3. Проверка текущего пароля → 4. Обновление → 5. Обновление сессии

4. Обработка ошибок
Подробные сообщения об ошибках от Identity
Логирование исключений
Сохранение введенных данных при ошибках

5. User Experience
Автоматический вход после регистрации
Запомнить меня
Сообщения об успехе через TempData
Безопасное перенаправление после входа
Этот контроллер реализует полный цикл работы с пользователями в соответствии с лучшими практиками безопасности ASP.NET Core Identity.
 */
