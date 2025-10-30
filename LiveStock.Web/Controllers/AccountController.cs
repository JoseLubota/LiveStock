using Microsoft.AspNetCore.Mvc;
using LiveStock.Web.Models;
using LiveStock.Core.Models;
using LiveStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LiveStock.Web.Service;

namespace LiveStock.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly adminService _adminService;
        private readonly LiveStockDbContext _context;

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes account controller with admin service and DB context"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public AccountController(adminService adminService, LiveStockDbContext context)
        {
            _adminService = adminService;
            _context = context;
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays the login view"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Authenticates admin or demo staff, sets session, and redirects"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Login(AdminModel model)
        {
            if (ModelState.IsValid)
            {
                int userID = _adminService.checkUser(model.Email, model.Password);
                if (userID > 0)
                {
                    HttpContext.Session.SetString("UserId", userID.ToString());
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction("Dashboard", "Management");
                }

                // Demo staff login (shared credentials)
                if (model.Email?.Trim().ToLower() == "staff@gmail.com" && model.Password == "staff123")
                {
                    // Pick the first active staff as session user context
                    var staffId = await _context.Staff.Where(s => s.IsActive).Select(s => s.Id).FirstOrDefaultAsync();
                    if (staffId == 0)
                    {
                        // If none found, create a minimal placeholder staff or fallback to 1
                        // For demo, fallback to 1 to avoid null session; ensure a staff exists in DB
                        staffId = 1;
                    }
                    HttpContext.Session.SetString("UserId", staffId.ToString());
                    HttpContext.Session.SetString("UserRole", "Employee");
                    return RedirectToAction("Dashboard", "Employee");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Clears the session and redirects to the home page"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}