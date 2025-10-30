using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LiveStock.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LiveStock.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly LiveStockDbContext _context;
        private readonly ILogger<EmployeeController> _logger;

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes employee controller with DB context"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public EmployeeController(LiveStockDbContext context, ILogger<EmployeeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows tasks, active staff, messages, and notes for the user"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToAction("Login", "Account");
                }

                var tasks = await _context.FarmTasks
                    .Include(t => t.AssignedTo)
                    .Include(t => t.CreatedBy)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                ViewBag.Staff = await _context.Staff.Where(s => s.IsActive).ToListAsync();

                ViewBag.Messages = await _context.Messages
                    .Include(m => m.Sender)
                    .OrderByDescending(m => m.SentAt)
                    .Take(100)
                    .ToListAsync();

                int userId = int.Parse(userIdStr);
                var notes = await _context.Notes
                    .Where(n => n.CreatedByUserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                ViewBag.Notes = notes;

                return View(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee dashboard");
                return StatusCode(500);
            }
        }
    }
}
