using FMassage.Data;
using FMassage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FMassage.Controllers
{
    [Authorize] // Требуем авторизацию для всего контроллера
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Booking - разный вид для админа и пользователей
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                // Админ видит ВСЕ брони с информацией о клиентах
                var allBookings = await _context.Bookings
                    .Include(b => b.User) // Загружаем данные пользователя
                    .ToListAsync();
                return View(allBookings);
            }
            else
            {
                // Обычный пользователь видит только ДОСТУПНЫЕ слоты
                var availableSlots = await _context.Bookings
                    .Where(b => b.IsAvailable)
                    .ToListAsync();
                return View(availableSlots);
            }
        }

        // GET: Booking/Create - только для админа
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Booking/Create - только для админа
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Create(DateTime slotDate, int durationInMinutes)
        {
            var booking = new Booking
            {
                SlotDate = slotDate,
                Duration = TimeSpan.FromMinutes(durationInMinutes),
                IsAvailable = true
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Edit/id - только для админа
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        // POST: Booking/Edit/id - только для админа
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Edit(int id, DateTime slotDate, int durationInMinutes)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.SlotDate = slotDate;
            booking.Duration = TimeSpan.FromMinutes(durationInMinutes);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Delete/id - только для админа
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        // POST: Booking/Delete/id - только для админа
        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Book/id - бронирование слота пользователем
        public async Task<IActionResult> Book(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null || !booking.IsAvailable)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Booking/Book/id - обработка бронирования
        [HttpPost]
        public async Task<IActionResult> Book(int id, string clientName, string phoneNumber, string comment)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null || !booking.IsAvailable)
            {
                return NotFound();
            }

            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);

            // Заполняем данные брони
            booking.IsAvailable = false;
            booking.ClientName = clientName;
            booking.PhoneNumber = phoneNumber;
            booking.Comment = comment;
            booking.UserId = user.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));
        }

        // GET: Booking/MyBookings - просмотр своих броней
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var userBookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(userBookings);
        }
    }
}