using FMassage.Data;
using FMassage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FMassage.Controllers
{
    public class MassageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MassageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Индексная страница для вывода списка услуг (GET-запрос) - доступно всем
        public async Task<IActionResult> Index()
        {
            var services = await _context.Massages.ToListAsync();
            return View(services);
        }

        // Открытие формы для создания новой услуги (GET-запрос) - только для Admin
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult Create()
        {
            return View();
        }

        // Сохранение новой услуги (POST-запрос) - только для Admin
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Create(Massage massage)
        {
            if (ModelState.IsValid)
            {
                _context.Massages.Add(massage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(massage);
        }

        // Редактирование существующей услуги (GET-запрос) - только для Admin
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Massages.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // Обработка изменений (POST-запрос) - только для Admin
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Edit(int id, Massage massage)
        {
            if (id != massage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(massage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(massage);
        }

        // Форма для удаления услуги (GET-запрос) - только для Admin
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Massages.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // Обработка удаления (POST-запрос) - только для Admin
        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Massages.FindAsync(id);
            if (service != null)
            {
                _context.Massages.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}