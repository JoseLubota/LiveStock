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

        public AccountController(adminService adminService)
        {
            _adminService = adminService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminModel model)
        {
            if (ModelState.IsValid)
            {
                int userID = _adminService.checkUser(model.Email, model.Password);
                if (userID > 0)
                {
                    HttpContext.Session.SetString("UserId", userID.ToString());
                    HttpContext.Session.SetString("UserRole", "Admin");
                    Console.WriteLine($"lOGIN WAS SUCCESFULL {userID}");
                    return RedirectToAction("Dashboard", "Management");
                }

            }
            else
            {
                Console.WriteLine($"lOGIN WAS Failed");
                ModelState.AddModelError("", "Invalid email or password");
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