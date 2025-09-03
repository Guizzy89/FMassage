using System.ComponentModel.DataAnnotations;

namespace FMassage.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // Дата и время слота
        [DataType(DataType.DateTime)]
        public DateTime SlotDate { get; set; }

        // Длительность сеанса
        public TimeSpan Duration { get; set; }

        // Доступность слота
        public bool IsAvailable { get; set; } = true;

        // Имя клиента
        [StringLength(100)]
        public string ClientName { get; set; } = "";

        // Телефон клиента
        [Phone]
        public string PhoneNumber { get; set; } = "";

        // Комментарий клиента
        public string Comment { get; set; } = "";
    }
}