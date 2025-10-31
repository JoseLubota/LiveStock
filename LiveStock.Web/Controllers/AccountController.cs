using Microsoft.AspNetCore.Mvc;
using LiveStock.Web.Models;
using LiveStock.Core.Models;
using LiveStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LiveStock.Web.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LiveStock.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly adminService _adminService;
        private readonly LiveStockDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes account controller with admin service and DB context"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public AccountController(adminService adminService, LiveStockDbContext context, ILogger<AccountController> logger, IConfiguration configuration)
        {
            _adminService = adminService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays the login view"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult Login()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing login view");
                return StatusCode(500);
            }
        }

        [HttpPost]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Authenticates admin or demo staff, sets session, and redirects"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Login(AdminModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int userID = _adminService.checkUser(model.Email, model.Password);
                    if (userID > 0)
                    {
                        HttpContext.Session.SetString("UserId", userID.ToString());
                        HttpContext.Session.SetString("UserRole", "Admin");
                        _logger.LogInformation("Admin login successful for {Email}", model.Email);
                        return RedirectToAction("Dashboard", "Management");
                    }

                    var demoStaffPassword = _configuration["DemoStaffPassword"];
                    if (model.Email?.Trim().ToLower() == "staff@gmail.com" && model.Password == demoStaffPassword)
                    {
                        var staffId = await _context.Staff.Where(s => s.IsActive).Select(s => s.Id).FirstOrDefaultAsync();
                        if (staffId == 0)
                        {
                            staffId = 1;
                        }
                        HttpContext.Session.SetString("UserId", staffId.ToString());
                        HttpContext.Session.SetString("UserRole", "Employee");
                        _logger.LogInformation("Demo staff login successful for staffId={StaffId}", staffId);
                        return RedirectToAction("Dashboard", "Employee");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login POST");
                return StatusCode(500);
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Clears the session and redirects to the home page"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                _logger.LogInformation("User logged out");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500);
            }
        }
    }
}