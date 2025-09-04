using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FMassage.Models
{
    public class User : IdentityUser
    {
        // Навигационное свойство для броней пользователя
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}