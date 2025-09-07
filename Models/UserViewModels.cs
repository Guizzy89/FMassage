using System.ComponentModel.DataAnnotations;

namespace FMassage.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string? ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string? CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новые пароли не совпадают")]
        public string? ConfirmPassword { get; set; }
    }
}

/*
 * Почему они нужны? Зачем не использовать сразу класс User?
 Ваша модель User (в базе данных) имеет много свойств, которые не нужны для регистрации или входа пользователя.
 А в форме регистрации вам нужно только: Email, Password, ConfirmPassword. Не нужно передавать в форму все поля пользователя!
 В модели User такие атрибуты были бы излишни - в базе нет поля "ConfirmPassword"!
 Безопасность: Вы не хотите, чтобы пользователь мог устанавливать свойства, которые не должны быть установлены напрямую (например, роли, права доступа и т.д.).
 Аналогия из реальной жизни
Модель User - это как ваша полная анкета в паспортном столе:
ФИО, дата рождения, адрес, серия паспорта, ИНН, СНИЛС...
ViewModel - это как бланк для конкретной услуги:
Для получения загранпаспорта: только ФИО и фото
Для голосования: только паспортные данные
Для банка: только ИНН и доход
Каждая форма запрашивает только нужные данные!

Преимущества этого подхода:
Безопасность - нет лишних полей
Чистота кода - каждая форма имеет свою модель
Валидация - специфичная для каждой формы
Гибкость - легко изменить форму без изменения основной модели
Производительность - передаются только нужные данные
Вывод: ViewModels - это не "лишние классы", а важный архитектурный паттерн, который делает ваше приложение более безопасным, чистым и поддерживаемым!
 */