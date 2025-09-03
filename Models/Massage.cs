using System.ComponentModel.DataAnnotations;

namespace FMassage.Models
{
    public class Massage
    {
        public int Id { get; set; }

        // Название услуги
        [Required(ErrorMessage = "Необходимо ввести название.")]
        public string Name { get; set; }

        // Описание услуги
        [Required(ErrorMessage = "Необходимо описать услугу.")]
        public string Description { get; set; }

        // Цена услуги
        [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть положительной.")]
        public decimal Price { get; set; }
    }
}
