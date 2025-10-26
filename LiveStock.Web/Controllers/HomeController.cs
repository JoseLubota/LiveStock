using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LiveStock.Web.Models;
using LiveStock.Infrastructure.Data;
using LiveStock.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LiveStock.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LiveStockDbContext _context;

    public HomeController(ILogger<HomeController> logger, LiveStockDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var model = new HomeSummaryViewModel();

        try
        {
            // Metrics
            model.LivestockCount = (_context.Sheep.Count()) + (_context.Cows.Count());
            model.ActiveStaffCount = _context.Staff.Count(s => s.IsActive);
            model.CampCount = _context.Camps.Count();

            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);
            model.RainfallThisWeekMl = _context.RainfallRecords
                .Where(r => r.RainfallDate >= sevenDaysAgo)
                .Sum(r => (double?)r.AmountMl) ?? 0.0;

            model.OpenTaskCount = _context.FarmTasks.Count(t => t.CompletedAt == null && (t.Status == "Pending" || t.Status == "In Progress"));

            // Net Profit (all-time)
            var totalIncome = _context.FinancialRecords
                .Where(f => f.Type == "Income")
                .Select(f => (decimal?)f.Amount)
                .Sum() ?? 0m;
            var totalExpenses = _context.FinancialRecords
                .Where(f => f.Type == "Expense")
                .Select(f => (decimal?)f.Amount)
                .Sum() ?? 0m;
            model.NetProfit = totalIncome - totalExpenses;

            // Highlights
            var today = DateTime.UtcNow.Date;
            var tasksDueToday = _context.FarmTasks.Count(t => t.CompletedAt == null && t.DueDate.Date <= today);
            if (tasksDueToday > 0)
                model.TodayHighlights.Add($"{tasksDueToday} task(s) due today");

            var rainfallToday = _context.RainfallRecords.Where(r => r.RainfallDate.Date == today).Sum(r => (double?)r.AmountMl) ?? 0.0;
            if (rainfallToday > 0)
                model.TodayHighlights.Add($"Rainfall recorded today: {rainfallToday:F1} ml");

            var recentNotes = _context.Notes
                .OrderByDescending(n => n.CreatedAt)
                .Take(3)
                .Select(n => new { n.Title, n.Category, n.CreatedAt })
                .ToList();

            foreach (var n in recentNotes)
            {
                model.RecentNotes.Add((n.Title, n.Category, n.CreatedAt.ToLocalTime().ToString("MMM d, HH:mm")));
            }

            // Role context from session
            model.IsAuthenticated = HttpContext.Session.Keys.Contains("UserId");
            model.UserRole = HttpContext.Session.GetString("UserRole");
            model.UserName = model.UserRole == "Admin" ? "Admin" : (model.UserRole == "Employee" ? "Staff" : null);

            // Announcements demo
            model.Announcements.Add("Scheduled maintenance on water pumps this Friday 10:00–12:00");
            model.Announcements.Add("New task workflow guide available in Help");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load HomeSummary");
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
