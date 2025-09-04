using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FMassage.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SlotDate { get; set; }

        public TimeSpan Duration { get; set; }
        public bool IsAvailable { get; set; } = true;

        [StringLength(100)]
        public string ClientName { get; set; } = "";

        [Phone]
        public string PhoneNumber { get; set; } = "";

        public string Comment { get; set; } = "";

        // Связь с пользователем, который забронировал
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        // Дата создания брони
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}