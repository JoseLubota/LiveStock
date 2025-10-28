using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LiveStock.Infrastructure.Data;

namespace LiveStock.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly LiveStockDbContext _context;

        public EmployeeController(LiveStockDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            // Current and completed tasks
            var tasks = await _context.FarmTasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Staff list for messaging
            ViewBag.Staff = await _context.Staff.Where(s => s.IsActive).ToListAsync();

            // Recent messages
            ViewBag.Messages = await _context.Messages
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Take(100)
                .ToListAsync();

            // Notes for current user
            int userId = int.Parse(userIdStr);
            var notes = await _context.Notes
                .Where(n => n.CreatedByUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            ViewBag.Notes = notes;

            return View(tasks);
        }
    }
}