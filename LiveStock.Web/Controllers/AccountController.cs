using Microsoft.AspNetCore.Mvc;
using LiveStock.Web.Models;
using LiveStock.Core.Models;
using LiveStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveStock.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly LiveStockDbContext _context;

        public AccountController(LiveStockDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var staff = await _context.Staff
                    .FirstOrDefaultAsync(s => s.EmployeeId == model.EmployeeId && s.IsActive);

                if (staff != null)
                {
                    // In a real application, you would hash and verify passwords
                    // For now, we'll use a simple check
                    if (staff.PhoneNumber.EndsWith(model.Password) || model.Password == "demo123")
                    {
                        // Set session or authentication cookie
                        HttpContext.Session.SetString("UserId", staff.Id.ToString());
                        HttpContext.Session.SetString("UserName", staff.Name);
                        HttpContext.Session.SetString("UserRole", staff.Role);

                        return RedirectToAction("Dashboard", "Management");
                    }
                }

                ModelState.AddModelError("", "Invalid Employee ID or Password");
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
} 