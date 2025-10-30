using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LiveStock.Core.Models;
using LiveStock.Web.Models;
using LiveStock.Web.ViewModels;
using LiveStock.Web.Service;
using LiveStock.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LiveStock.Web.Controllers
{
    public class ManagementController : Controller
    {
        private readonly LiveStockDbContext _context;
        private readonly sheepService _sheepService;
        private readonly cowService _cowService;
        private readonly INoteService _noteService;
        private readonly ILogger<ManagementController> _logger;

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes management controller with DbContext and livestock/note services"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public ManagementController(LiveStockDbContext context, sheepService sheepService, cowService cowService, INoteService noteService, ILogger<ManagementController> logger)
        {
            _context = context;
            _sheepService = sheepService;
            _cowService = cowService;
            _noteService = noteService;
            _logger = logger;
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays the management dashboard with livestock, staff, task, and rainfall summaries"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult Dashboard()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var dashboardViewModel = new DashboardViewModel
                {
                    TotalSheep = _sheepService.GetAllSheep().Count(c => c.IsActive),
                    TotalCows = _cowService.GetAllCow().Count(c => c.IsActive),
                    TotalStaff = _context.Staff.Count(s => s.IsActive),
                    PendingTasks = _context.FarmTasks.Count(t => t.Status == "Pending"),
                    RecentRainfall = _context.RainfallRecords
                        .Include(r => r.Camp)
                        .OrderByDescending(r => r.RainfallDate)
                        .Take(5)
                        .ToList()
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading management dashboard");
                return StatusCode(500);
            }
        }

        #region Sheep Management
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows the Add Sheep form with available camps"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult AddSheep()
        {
            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            return View();
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Lists all active sheep ordered by Id"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Sheep()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var sheepList = _sheepService.GetAllSheep()
                .OrderBy(s => s.Id)
                .ToList();

            return View(sheepList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Creates a new sheep, handles optional photo upload, and records note"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_    
        public async Task<IActionResult> AddSheep(string Breed, int Camp, string Gender, DateOnly BirthDate, string? Notes, IFormFile? Photo, decimal Price)
        {
            string photoURL = string.Empty;
            if (Photo != null)
            {
                photoURL = await _sheepService.SaveSheepPhoto(Photo);
            }

            var newSheepId = _sheepService.AddSheep(
                breed: Breed,
                birthDate: BirthDate,
                camp: Camp,
                createdAt: DateTime.UtcNow,
                gender: Gender,
                price: Price,
                notes: Notes,
                photoURL: photoURL);

            if (!string.IsNullOrWhiteSpace(Notes))
            {
                await _noteService.CreateNoteAsync(
                    userId: int.Parse(HttpContext.Session.GetString("UserId")),
                    title: $"Added New Sheep - {newSheepId}",
                    content: Notes,
                    category: "Sheep",
                    createdAt: DateTime.UtcNow
                );
            }

            return RedirectToAction("Sheep");

        }
         
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows sheep details including medical records and camp movements"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> SheepDetails(int id)
        {
            var sheep = _sheepService.GetAllSheep().FirstOrDefault(s => s.Id == id);

            if (sheep == null)
            {
                return NotFound();
            }
             
            try
            {
                sheep.MedicalRecords = await _context.MedicalRecords
                    .Where(m => m.AnimalType == "Sheep" && m.SheepId == id)
                    .OrderByDescending(m => m.TreatmentDate)
                    .ToListAsync();

                sheep.CampMovements = await _context.CampMovements
                    .Include(cm => cm.FromCamp)
                    .Include(cm => cm.ToCamp)
                    .Where(cm => cm.AnimalType == "Sheep" && cm.AnimalId == id)
                    .OrderByDescending(cm => cm.MovementDate)
                    .ToListAsync();
            }
            catch {}

            ViewBag.Camps = await _context.Camps.OrderBy(c => c.CampNumber).ToListAsync();

            return View(sheep);
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes a sheep after removing related medical records to avoid FK conflicts"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> RemoveSheep(int id)
        {
            try
            {
                // Delete dependent medical records to avoid FK conflicts, then hard-delete sheep
                await _context.MedicalRecords
                    .Where(m => m.AnimalType == "Sheep" && m.SheepId == id)
                    .ExecuteDeleteAsync();

                _sheepService.DeleteSheep(id);
                return RedirectToAction(nameof(Sheep));

            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Prepares sheep edit view with camps and selected sheep context"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult Sheep_Edit_Details(int id)
        {
            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates sheep details, manages photo upload/deletion, and records update note"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> UpdateSheep(int id, string Breed, int Camp, string Gender, DateOnly BirthDate, string? Notes, IFormFile? Photo, decimal Price)
        {
            Sheep newSheep = new Sheep();
            newSheep.Id = id;
            newSheep.Breed = Breed;
            newSheep.CampId = Camp;
            newSheep.Gender = Gender;
            newSheep.BirthDate = BirthDate;
            newSheep.Price = Price;
            newSheep.UpdatedAt = DateTime.UtcNow;
            newSheep.Notes = Notes;

            if (Photo != null)
            {
                _sheepService.DeleteSheepPhoto(id);
                newSheep.PhotoUrl = await _sheepService.SaveSheepPhoto(Photo);
            }
            // Load current record using domain SheepID
            var currentSheep = _sheepService.getSheepById(newSheep.Id);

            var newSheepList = new Queue<Sheep>();
            newSheepList.Enqueue(newSheep);
            var mergedSheepList = _sheepService.FillVoidSheppFields(currentSheep, newSheepList);
            var merged = mergedSheepList.FirstOrDefault();
            _sheepService.UpdateSheep(merged);

            if (!string.IsNullOrWhiteSpace(newSheep.Notes))
            {
                await _noteService.CreateNoteAsync(
                    userId: int.Parse(HttpContext.Session.GetString("UserId")),
                    title: $"Updated Sheep - {merged.Id}",
                    content: newSheep.Notes,
                    category: "Sheep",
                    createdAt: DateTime.UtcNow
                );
            }
            return RedirectToAction("Sheep");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Performs batch actions (e.g., deactivate, move) on selected sheep"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult SheepBulkActions(string action, string reason, HashSet<int> selectedId)
        {
            if(selectedId == null || selectedId.Count == 0)
            {
                return RedirectToAction(nameof(Sheep));
            }
 
            _sheepService.SheepBulkActions(action, reason, selectedId);
            return RedirectToAction(nameof(Sheep));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Generates a PDF report of sheep and returns it as a file"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult GenerateSheepReport()
        {
            var file = _sheepService.GenerateSheepReport();
            return File(file, "application/pdf", "SheepReport.pdf");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Exports sheep data to CSV and returns the file"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult ExportSheep()
        {
            var result = _sheepService.ExportSheep();

            return File(result, "text/csv", "SheepData.csv");
        }
        #endregion

        #region Cow Management
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Lists all cows ordered by Id"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Cows()
        {
            var cows = _cowService.GetAllCow()
                            .OrderBy(c => c.Id)
                            .ToList();

            return View(cows);
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows the Add Cow form with camp selection"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult AddCow()
        {
            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a new cow, handles photo upload, and records note when present"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddCow(Cow cow, IFormFile Photo)
        {
            cow.PhotoUrl = string.Empty;
            if (Photo != null)
                cow.PhotoUrl = await _cowService.SaveCowPhoto(Photo);

            _cowService.AddCow(breed: cow.Breed,
                earTag: cow.EarTag,
                birdthDate: cow.BirthDate,
                camp: cow.CampId,
                createdAt: DateTime.UtcNow,
                gender: cow.Gender,
                price: cow.Price,
                photoURL: cow.PhotoUrl,
                IsPregnant: cow.IsPregnant,
                expectedCalvingDate: cow.ExpectedCalvingDate,
                notes : cow.Notes
            );

            if (cow.Notes != null)
            {
                await _noteService.CreateNoteAsync(
                    userId : int.Parse(HttpContext.Session.GetString("UserId")),
                    title : $"Added New Cow - {cow.EarTag}",
                    content : cow.Notes,
                    category : "Cow",
                    createdAt : DateTime.UtcNow
                );
            }

            return RedirectToAction("Cows");
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows cow details including medical records and camp movements"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> CowDetails(int id)
        {
            var cow = _cowService.GetAllCow().FirstOrDefault(s => s.Id == id);

            if (cow == null)
            {
                return NotFound();
            }
            try
            {
                cow.MedicalRecords = await _context.MedicalRecords
                    .Where(m => m.AnimalType == "Cow" && m.CowId == id)
                    .OrderByDescending(m => m.TreatmentDate)
                    .ToListAsync();

                cow.CampMovements = await _context.CampMovements
                    .Include(cm => cm.FromCamp)
                    .Include(cm => cm.ToCamp)
                    .Where(cm => cm.AnimalType == "Cow" && cm.AnimalId == id)
                    .OrderByDescending(cm => cm.MovementDate)
                    .ToListAsync();
            }
            catch {}

            ViewBag.Camps = await _context.Camps.OrderBy(c => c.CampNumber).ToListAsync();

            return View(cow);
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes a cow after removing related medical records to avoid FK conflicts"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> RemoveCow(int id)
        {
            try
            {
                // Delete related medical records first to avoid foreign key conflicts
                await _context.MedicalRecords
                    .Where(m => m.AnimalType == "Cow" && m.CowId == id)
                    .ExecuteDeleteAsync();

                // Hard delete the cow
                _cowService.DeleteCow(id);
                return RedirectToAction(nameof(Cows));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Prepares cow edit view with camps and selected cow context"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult CowEditDetails(int id)
        {
            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            ViewBag.CowID = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates cow details, manages photo upload/deletion, and records update note"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> UpdateCow(Cow cow, IFormFile? Photo)
        {
            Cow newCow = new()
            {
                Id = cow.Id,
                EarTag = cow.EarTag,
                Breed = cow.Breed,
                CampId = cow.CampId,
                Gender = cow.Gender,
                BirthDate = cow.BirthDate,
                Price = cow.Price,
                UpdatedAt = DateTime.UtcNow,
                IsPregnant = cow.IsPregnant,
                ExpectedCalvingDate = cow.IsPregnant ? cow.ExpectedCalvingDate : null,
                Notes = cow.Notes
            };


            if (Photo != null)
            {
                _cowService.DeleteCowPhoto(cow.Id);
                newCow.PhotoUrl = await _cowService.SaveCowPhoto(Photo);
            }
            var currentCow = _cowService.getCowByID(newCow.Id);

            var newCowList = new Queue<Cow>();
            newCowList.Enqueue(newCow);

            var mergedCowList = _cowService.FillVoidCowFields(currentCow, newCowList);
            _cowService.UpdateCow(mergedCowList.First());

            if (newCow.Notes != null)
            {
                await _noteService.CreateNoteAsync(
                    userId: int.Parse(HttpContext.Session.GetString("UserId")),
                    title: $"Updated Cow - {mergedCowList.First().EarTag}",
                    content: newCow.Notes,
                    category: "Cow",
                    createdAt: DateTime.UtcNow
                );
            }



            return RedirectToAction("Cows");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Uploads a new sheep photo, updates its URL, and saves changes"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> UpdateSheepPhoto(int id, IFormFile Photo)
        {
            if (Photo == null)
            {
                return BadRequest("No photo uploaded");
            }

            _sheepService.DeleteSheepPhoto(id);
            var url = await _sheepService.SaveSheepPhoto(Photo);

            var sheep = await _context.Sheep.FindAsync(id);
            if (sheep == null)
            {
                return NotFound();
            }
            sheep.PhotoUrl = url;
            sheep.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SheepDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Uploads a new cow photo, updates its URL, and saves changes"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> UpdateCowPhoto(int id, IFormFile Photo)
        {
            if (Photo == null)
            {
                return BadRequest("No photo uploaded");
            }

            _cowService.DeleteCowPhoto(id);
            var url = await _cowService.SaveCowPhoto(Photo);

            var cow = await _context.Cows.FindAsync(id);
            if (cow == null)
            {
                return NotFound();
            }
            cow.PhotoUrl = url;
            cow.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CowDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates cow pregnancy status and expected calving date"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> UpdatePregnancyStatus(int id, bool isPregnant, DateTime? expectedCalvingDate)
        {
            var cow = await _context.Cows.FindAsync(id);
            if (cow == null)
            {
                return NotFound();
            }
            cow.IsPregnant = isPregnant;
            cow.ExpectedCalvingDate = isPregnant ? expectedCalvingDate : null;
            cow.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CowDetails), new { id });
        }
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Performs batch actions (e.g., deactivate, move) on selected cows"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult CowBulkActions(string action, string reason, HashSet<int> selectedCowID)
        {
            if (selectedCowID == null || selectedCowID.Count == 0)
            {
                return RedirectToAction(nameof(Cows));
            }

            _cowService.CowBulkActions(action, reason, selectedCowID);
            return RedirectToAction(nameof(Cows));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Generates a PDF report of cows and returns it as a file"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult GenerateCowReport()
        {
            var file = _cowService.GenerateCowReport();
            return File(file, "application/pdf", "CowReport.pdf");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Exports cow data to CSV and returns the file"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult ExportCow()
        {
            var result = _cowService.ExportCow();

            return File(result, "text/csv", "CowData.csv");
        }
        #endregion

        #region Camp Management
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays all camps with their animals ordered by camp number"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Camps()
        {
            var camps = await _context.Camps
                .Include(c => c.Sheep)
                .Include(c => c.Cows)
                .OrderBy(c => c.CampNumber)
                .ToListAsync();

            return View(camps);
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows camp details including animals and rainfall history"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> CampDetails(int id)
        {
            var camp = await _context.Camps
                .Include(c => c.Sheep)
                .Include(c => c.Cows)
                .Include(c => c.RainfallRecords.OrderByDescending(r => r.RainfallDate))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (camp == null)
            {
                return NotFound();
            }

            ViewBag.Camps = await _context.Camps.OrderBy(c => c.CampNumber).ToListAsync();
            return View(camp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Moves a sheep or cow between camps and records the movement"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> MoveAnimal(int animalId, string animalType, int fromCampId, int toCampId, string? reason)
        {
            var movement = new CampMovement
            {
                AnimalId = animalId,
                AnimalType = animalType,
                FromCampId = fromCampId,
                ToCampId = toCampId,
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Reason = reason
            };

            _context.CampMovements.Add(movement);

            // Update animal's current camp
            if (animalType == "Sheep")
            {
                var sheep = await _context.Sheep.FindAsync(animalId);
                if (sheep != null)
                {
                    sheep.CampId = toCampId;
                    sheep.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (animalType == "Cow")
            {
                var cow = await _context.Cows.FindAsync(animalId);
                if (cow != null)
                {
                    cow.CampId = toCampId;
                    cow.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            // Redirect to the relevant details page when moving from animal details
            if (Request.Headers.ContainsKey("Referer") && Request.Headers["Referer"].ToString().Contains("SheepDetails"))
            {
                return RedirectToAction(nameof(SheepDetails), new { id = animalId });
            }
            if (Request.Headers.ContainsKey("Referer") && Request.Headers["Referer"].ToString().Contains("CowDetails"))
            {
                return RedirectToAction(nameof(CowDetails), new { id = animalId });
            }
            return RedirectToAction(nameof(CampDetails), new { id = toCampId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a rainfall record to a camp; supports AJAX and redirects"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddRainfallForCamp(int campId, double amountMl, string? notes, DateTime rainfallDate)
        {
            try
            {
                var camp = await _context.Camps.FindAsync(campId);
                if (camp == null)
                {
                    return NotFound($"Camp not found: {campId}");
                }

                if (amountMl < 0)
                {
                    amountMl = 0; // sanitize negative input
                }

                if (rainfallDate == default)
                {
                    rainfallDate = DateTime.UtcNow.Date;
                }

                var rainfall = new RainfallRecord
                {
                    CampId = campId,
                    RainfallDate = rainfallDate,
                    AmountMl = amountMl,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RainfallRecords.Add(rainfall);
                await _context.SaveChangesAsync();

                // If this is an AJAX request, return JSON instead of redirect
                if (Request.Headers.ContainsKey("X-Requested-With") ||
                    (Request.Headers.ContainsKey("Accept") && Request.Headers["Accept"].ToString().Contains("application/json")))
                {
                    return Ok(new { success = true, campId, amountMl, rainfallDate, notes });
                }

                return RedirectToAction(nameof(CampDetails), new { id = campId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddRainfallForCamp failed for campId={campId}");
                return BadRequest("Failed to add rainfall record.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Clears all rainfall records for a camp; supports AJAX and redirects"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> ClearRainfallForCamp(int campId)
        {
            try
            {
                var camp = await _context.Camps.FindAsync(campId);
                if (camp == null)
                {
                    return NotFound($"Camp not found: {campId}");
                }

                var records = _context.RainfallRecords.Where(r => r.CampId == campId);
                _context.RainfallRecords.RemoveRange(records);
                await _context.SaveChangesAsync();

                // Support AJAX and regular form posts
                if (Request.Headers.ContainsKey("X-Requested-With") ||
                    (Request.Headers.ContainsKey("Accept") && Request.Headers["Accept"].ToString().Contains("application/json")))
                {
                    return Ok(new { success = true, campId });
                }

                return RedirectToAction(nameof(CampDetails), new { id = campId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearRainfallForCamp failed for campId={campId}");
                return BadRequest("Failed to clear rainfall history.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates camp properties (name, number, hectares, description) and saves"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> UpdateCamp(int id, string Name, int CampNumber, double Hectares, string? Description)
        {
            var camp = await _context.Camps.FindAsync(id);
            if (camp == null)
            {
                return NotFound();
            }

            camp.Name = Name;
            camp.CampNumber = CampNumber;
            camp.Hectares = Hectares;
            camp.Description = Description;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CampDetails), new { id });
        }
        #endregion

        #region Staff Management
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Lists active staff members"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Staff()
        {
            var staff = await _context.Staff
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(staff);
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows the Add Staff form"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult AddStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a staff member, sets defaults, and saves"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddStaff(Staff staff)
        {
            if (ModelState.IsValid)
            {
                staff.CreatedAt = DateTime.UtcNow;
                staff.IsActive = true;
                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Staff));
            }

            return View(staff);
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows the edit view for a staff member"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> EditStaff(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null || !staff.IsActive)
            {
                return NotFound();
            }
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates staff details and persists changes"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> EditStaff(Staff staff)
        {
            if (ModelState.IsValid)
            {
                var existingStaff = await _context.Staff.FindAsync(staff.Id);
                if (existingStaff != null && existingStaff.IsActive)
                {
                    existingStaff.Name = staff.Name;
                    existingStaff.EmployeeId = staff.EmployeeId;
                    existingStaff.PhoneNumber = staff.PhoneNumber;
                    existingStaff.Email = staff.Email;
                    existingStaff.Role = staff.Role;
                    existingStaff.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Staff));
                }
            }
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Soft-deletes a staff member by marking them inactive"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null && staff.IsActive)
            {
                // Soft delete - mark as inactive
                staff.IsActive = false;
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Staff));
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows tasks assigned to the specified staff member"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> StaffTasks(int staffId)
        {
            var tasks = await _context.FarmTasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == staffId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tasks);
        }
        #endregion

        #region Task Management
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays all farm tasks along with staff and recent messages"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Tasks()
        {
            try
            {
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

                return View(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Tasks view");
                return StatusCode(500);
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows the Create Task form with active staff list"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult CreateTask()
        {
            try
            {
                ViewBag.Staff = _context.Staff.Where(s => s.IsActive).ToList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing CreateTask form");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Creates a new task; sets CreatedById, timestamps, validates, and saves"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> CreateTask(FarmTask task)
        {
            try
            {
                _logger.LogInformation("CreateTask received: Description={Description}, AssignedToId={AssignedToId}, Importance={Importance}, DueDate={DueDate}, Notes={Notes}",
                    task.Description, task.AssignedToId, task.Importance, task.DueDate, task.Notes);

                var activeStaff = _context.Staff.Where(s => s.IsActive).ToList();
                _logger.LogInformation("Active staff count: {Count}", activeStaff.Count);

                var userIdStr = HttpContext.Session.GetString("UserId");
                int createdById;
                if (!int.TryParse(userIdStr, out createdById))
                {
                    var fallbackStaff = activeStaff.FirstOrDefault();
                    createdById = task.AssignedToId != 0
                        ? task.AssignedToId
                        : (fallbackStaff != null ? fallbackStaff.Id : 0);
                }

                _logger.LogInformation("Setting CreatedById to: {CreatedById}", createdById);
                task.CreatedById = createdById;
                task.CreatedAt = DateTime.UtcNow;
                task.Status = "Pending";

                _logger.LogInformation("Final task values: AssignedToId={AssignedToId}, CreatedById={CreatedById}", task.AssignedToId, task.CreatedById);

                if (ModelState.IsValid)
                {
                    _context.FarmTasks.Add(task);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Tasks));
                }

                foreach (var kvp in ModelState)
                {
                    foreach (var err in kvp.Value.Errors)
                    {
                        _logger.LogError("CreateTask validation error: {Key} - {Error}", kvp.Key, err.ErrorMessage);
                    }
                }

                ViewBag.Staff = activeStaff;
                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Marks a task as completed and redirects appropriately by role"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            try
            {
                var task = await _context.FarmTasks.FindAsync(taskId);
                if (task != null)
                {
                    task.Status = "Completed";
                    task.CompletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                var role = HttpContext.Session.GetString("UserRole");
                if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Employee");
                }
                return RedirectToAction(nameof(Tasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task Id={TaskId}", taskId);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates a task's status and sets or clears completion timestamp"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> UpdateTaskStatus(int taskId, string status)
        {
            try
            {
                var task = await _context.FarmTasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound();
                }

                var allowed = new[] { "Pending", "In Progress", "Completed" };
                if (!allowed.Contains(status))
                {
                    return BadRequest("Invalid status");
                }

                task.Status = status;
                if (status == "Completed")
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                else
                {
                    task.CompletedAt = null;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Tasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status Id={TaskId} Status={Status}", taskId, status);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes a completed task (admin-only) and redirects to task list"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                var task = await _context.FarmTasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound();
                }

                if (!string.Equals(task.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only completed tasks can be deleted.");
                }

                _context.FarmTasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Tasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task Id={TaskId}", taskId);
                return StatusCode(500);
            }
        }
        #endregion

        #region Water/Rainfall Management
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays water overview with camps and rainfall records"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Water()
        {
            try
            {
                var camps = await _context.Camps
                    .OrderBy(c => c.CampNumber)
                    .ToListAsync();

                var rainfallRecords = await _context.RainfallRecords
                    .Include(r => r.Camp)
                    .OrderByDescending(r => r.RainfallDate)
                    .ToListAsync();

                var model = new LiveStock.Web.ViewModels.WaterViewModel
                {
                    Camps = camps,
                    RainfallRecords = rainfallRecords
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Water view");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a rainfall measurement to a camp and saves"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddRainfall(int campId, double amountMl)
        {
            try
            {
                var rainfall = new RainfallRecord
                {
                    CampId = campId,
                    RainfallDate = DateTime.UtcNow,
                    AmountMl = amountMl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RainfallRecords.Add(rainfall);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Water));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rainfall campId={CampId}", campId);
                return StatusCode(500);
            }
        }
        #endregion

        #region Assets & Finance
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays assets ordered by category and name"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Assets()
        {
            var assets = await _context.Assets
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return View(assets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds an asset after validation; logs and redirects to list"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddAsset(Asset asset)
        {
            try
            {
                _logger.LogInformation("AddAsset received: Name={Name}, Category={Category}, Type={Type}, Quantity={Quantity}, Unit={Unit}, PurchasePrice={Price}, PurchaseDate={PurchaseDate}, Location={Location}",
                    asset.Name, asset.Category, asset.Type, asset.Quantity, asset.Unit, asset.PurchasePrice, asset.PurchaseDate, asset.Location);

                if (!ModelState.IsValid)
                {
                    foreach (var kvp in ModelState)
                    {
                        foreach (var err in kvp.Value.Errors)
                        {
                            _logger.LogError("AddAsset validation error: {Key} - {Error}", kvp.Key, err.ErrorMessage);
                        }
                    }
                    var assets = await _context.Assets
                        .OrderBy(a => a.Category)
                        .ThenBy(a => a.Name)
                        .ToListAsync();
                    TempData["AssetError"] = "Could not add asset. Please check required fields.";
                    return View(nameof(Assets), assets);
                }

                asset.Status = string.IsNullOrWhiteSpace(asset.Status) ? "Active" : asset.Status;
                asset.CreatedAt = DateTime.UtcNow;
                asset.UpdatedAt = null;

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Asset saved with Id={Id}", asset.Id);
                return RedirectToAction(nameof(Assets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding asset");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Removes an asset by id if found, then redirects"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> RemoveAsset(int id)
        {
            try
            {
                _logger.LogInformation("RemoveAsset requested for Id={Id}", id);
                var asset = await _context.Assets.FindAsync(id);
                if (asset != null)
                {
                    _context.Assets.Remove(asset);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Asset deleted Id={Id}", id);
                }
                else
                {
                    _logger.LogInformation("Asset not found Id={Id}", id);
                }
                return RedirectToAction(nameof(Assets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing asset Id={Id}", id);
                return StatusCode(500);
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows the edit view for a single asset"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> EditAsset(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates asset fields and saves changes"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> EditAsset(Asset input)
        {
            try
            {
                _logger.LogInformation("EditAsset incoming update Id={Id}", input.Id);
                if (!ModelState.IsValid)
                {
                    foreach (var kvp in ModelState)
                    {
                        foreach (var err in kvp.Value.Errors)
                        {
                            _logger.LogError("EditAsset validation error: {Key} - {Error}", kvp.Key, err.ErrorMessage);
                        }
                    }
                    return View(input);
                }

                var asset = await _context.Assets.FindAsync(input.Id);
                if (asset == null)
                {
                    return NotFound();
                }

                asset.Name = input.Name;
                asset.Category = input.Category;
                asset.Type = input.Type;
                asset.Description = input.Description;
                asset.Quantity = input.Quantity;
                asset.Unit = input.Unit;
                asset.PurchasePrice = input.PurchasePrice;
                asset.PurchaseDate = input.PurchaseDate;
                asset.ExpiryDate = input.ExpiryDate;
                asset.Status = string.IsNullOrWhiteSpace(input.Status) ? asset.Status : input.Status;
                asset.Location = input.Location;
                asset.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Asset updated Id={Id}", asset.Id);
                return RedirectToAction(nameof(Assets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing asset Id={Id}", input.Id);
                return StatusCode(500);
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays financial records ordered by transaction date"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> Finance()
        {
            var financialRecords = await _context.FinancialRecords
                .OrderByDescending(f => f.TransactionDate)
                .ToListAsync();

            return View(financialRecords);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a financial record after validation and timestamps"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddFinancialRecord(LiveStock.Core.Models.FinancialRecord record)
        {
            if (!ModelState.IsValid)
            {
                TempData["FinanceError"] = "Please correct the form and try again.";
                return RedirectToAction(nameof(Finance));
            }
            record.CreatedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;
            _context.FinancialRecords.Add(record);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Finance));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes a financial record by id and redirects"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> DeleteFinancialRecord(int id)
        {
            var existing = await _context.FinancialRecords.FindAsync(id);
            if (existing == null)
            {
                TempData["FinanceError"] = "Record not found.";
                return RedirectToAction(nameof(Finance));
            }
            _context.FinancialRecords.Remove(existing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Finance));
        }

        [HttpGet]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Shows the edit form for a financial record"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> EditFinancialRecord(int id)
        {
            var record = await _context.FinancialRecords.FindAsync(id);
            if (record == null)
            {
                return RedirectToAction(nameof(Finance));
            }
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates a financial record's fields and saves"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> EditFinancialRecord(LiveStock.Core.Models.FinancialRecord record)
        {
            if (!ModelState.IsValid)
            {
                return View(record);
            }

            var existing = await _context.FinancialRecords.FindAsync(record.Id);
            if (existing == null)
            {
                return RedirectToAction(nameof(Finance));
            }

            existing.Type = record.Type;
            existing.Description = record.Description;
            existing.Amount = record.Amount;
            existing.TransactionDate = record.TransactionDate;
            existing.Category = record.Category;
            existing.Reference = record.Reference;
            existing.Notes = record.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Finance));
        }
                
        #region Notes
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Displays the current user's notes, optionally filtered by category"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult Notes(string? category)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToAction("Login", "Account");
                }
                int userId = int.Parse(userIdStr);
                var query = _context.Notes
                    .Where(n => n.CreatedByUserId == userId);

                if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(n => n.Category == category);
                }

                var notes = query
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                ViewBag.ActiveCategory = string.IsNullOrWhiteSpace(category) ? "All" : category;
                return View(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Notes category={Category}", category);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Creates a note for the current user after model validation"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult CreateNote([Bind("Title,Content,Category")] Note input)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToAction("Login", "Account");
                }
                if (!ModelState.IsValid)
                {
                    var roleInvalid = HttpContext.Session.GetString("UserRole");
                    if (string.Equals(roleInvalid, "Employee", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Dashboard", "Employee");
                    }
                    return RedirectToAction("Notes");
                }
                int userId = int.Parse(userIdStr);
                var note = new Note
                {
                    Title = input.Title,
                    Content = input.Content,
                    Category = input.Category,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notes.Add(note);
                _context.SaveChanges();

                var role = HttpContext.Session.GetString("UserRole");
                if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Employee");
                }
                return RedirectToAction("Notes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note");
                return StatusCode(500);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes the current user's note by id"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public IActionResult DeleteNote(int id)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToAction("Login", "Account");
                }
                int userId = int.Parse(userIdStr);

                var note = _context.Notes.FirstOrDefault(n => n.Id == id && n.CreatedByUserId == userId);
                if (note != null)
                {
                    _context.Notes.Remove(note);
                    _context.SaveChanges();
                }

                return RedirectToAction(nameof(Notes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note Id={Id}", id);
                return StatusCode(500);
            }
        }
        #endregion
        // Livestock
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a sheep or cow to a camp with defaults and saves"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddLivestockToCamp(int campId, string animalType, string Breed, DateOnly BirthDate, string Gender, decimal Price, string? EarTag, bool? IsPregnant, DateTime? ExpectedCalvingDate, string? Notes)
        {
            try
            {
                if (animalType == "Sheep")
                {
                    var sheep = new Sheep
                    {
                        Breed = Breed,
                        BirthDate = BirthDate,
                        CampId = campId,
                        Gender = Gender,
                        Price = Price,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        Notes = Notes
                    };

                    _context.Sheep.Add(sheep);
                    await _context.SaveChangesAsync();
                }
                else if (animalType == "Cow")
                {
                    var cow = new Cow
                    {
                        Breed = Breed,
                        BirthDate = BirthDate,
                        CampId = campId,
                        Gender = Gender,
                        Price = Price,
                        Status = "Active",
                        EarTag = EarTag,
                        IsPregnant = IsPregnant ?? false,
                        ExpectedCalvingDate = ExpectedCalvingDate,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        Notes = Notes
                    };

                    _context.Cows.Add(cow);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest("Unsupported animal type.");
                }

                return RedirectToAction(nameof(CampDetails), new { id = campId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding livestock to campId={CampId} animalType={AnimalType}", campId, animalType);
                return StatusCode(500);
            }
        }

        // Chat/Staff Communication
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Sends a staff message or broadcast, resolving sender and persisting"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> SendMessage(string content, int? recipientId, int? senderId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["MessageError"] = "Message cannot be empty.";
                    return RedirectToAction(nameof(Tasks));
                }

                int resolvedSenderId = 0;
                if (senderId.HasValue && senderId.Value > 0)
                {
                    resolvedSenderId = senderId.Value;
                }
                else
                {
                    var userIdStr = HttpContext.Session.GetString("UserId");
                    if (!int.TryParse(userIdStr, out resolvedSenderId))
                    {
                        resolvedSenderId = await _context.Staff.Where(s => s.IsActive).Select(s => s.Id).FirstOrDefaultAsync();
                    }
                }

                if (resolvedSenderId == 0)
                {
                    TempData["MessageError"] = "No active staff found to send message.";
                    return RedirectToAction(nameof(Tasks));
                }

                var message = new LiveStock.Core.Models.Message
                {
                    SenderId = resolvedSenderId,
                    RecipientId = recipientId,
                    Content = content,
                    IsBroadcast = recipientId == null,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                var role = HttpContext.Session.GetString("UserRole");
                if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Employee");
                }
                return RedirectToAction(nameof(Tasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to recipientId={RecipientId}", recipientId);
                return StatusCode(500);
            }
        }
        #endregion

        #region Medicine
        [HttpPost]
        [ValidateAntiForgeryToken]
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a medical record for a sheep or cow and redirects to details"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IActionResult> AddMedicalRecord(string animalType, DateTime TreatmentDate, string Treatment, string? Veterinarian, decimal? Cost, string? Notes, int? SheepId, int? CowId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(animalType) || string.IsNullOrWhiteSpace(Treatment))
                {
                    return BadRequest("Missing required fields");
                }

            var record = new MedicalRecord
            {
                AnimalType = animalType,
                TreatmentDate = TreatmentDate,
                Treatment = Treatment,
                Veterinarian = Veterinarian,
                Cost = Cost,
                Notes = Notes,
                CreatedAt = DateTime.UtcNow,
                SheepId = SheepId,
                CowId = CowId
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            if (animalType == "Sheep")
                return RedirectToAction(nameof(SheepDetails), new { id = SheepId });
            if (animalType == "Cow")
                return RedirectToAction(nameof(CowDetails), new { id = CowId });
            return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medical record for animalType={AnimalType}", animalType);
                return StatusCode(500);
            }
        }

        #endregion
    }
}
