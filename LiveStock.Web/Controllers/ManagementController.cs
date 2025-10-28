using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LiveStock.Infrastructure.Data;
using LiveStock.Core.Models;
using LiveStock.Web.Models;
using LiveStock.Web.ViewModels;
using LiveStock.Web.Service;
using Microsoft.IdentityModel.Tokens;

namespace LiveStock.Web.Controllers
{
    public class ManagementController : Controller
    {
        private readonly LiveStockDbContext _context;
        private readonly sheepService _sheepService;

        public ManagementController(LiveStockDbContext context, sheepService sheepService)
        {
            _context = context;
            _sheepService = sheepService;
        }

        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var dashboardViewModel = new DashboardViewModel
            {
                TotalSheep = _context.Sheep.Count(s => s.IsActive),
                TotalCows = _context.Cows.Count(c => c.IsActive),
                TotalStaff = _context.Staff.Count(s => s.IsActive),
                PendingTasks = _context.FarmTasks.Count(t => t.Status == "Pending"),
                RecentRainfall = _context.RainfallRecords
                    .OrderByDescending(r => r.RainfallDate)
                    .Take(5)
                    .ToList()
            };

            return View(dashboardViewModel);
        }

        #region Sheep Management

        public IActionResult AddSheep()
        {
            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            return View();
        }
        public async Task<IActionResult> Sheep()
        {
            var sheepList = _sheepService.GetAllSheep()
                .OrderBy(s => s.SheepID)
                .ToList();

            return View(sheepList);
        }

        [HttpPost]
        public async Task<IActionResult> AddSheep(string Breed, int Camp, string Gender, DateOnly BirthDate, string? Notes, IFormFile? Photo, decimal Price)
        {
            int? PhotoID = null;
            if (Photo != null)
            {
                // Save photo in database
                // Genererate and get photo ID
            }

            _sheepService.AddSheep(breed: Breed,
                birdthDate: BirthDate,
                camp: Camp,
                gender: Gender,
                price: Price,
                photoID: PhotoID);

            if (Notes != null)
            {
                // get sheep ID
                // create a new note
            }

            return RedirectToAction("Sheep");

        }
         
        public async Task<IActionResult> SheepDetails(int id)
        {
            /*
             *                 .Include(s => s.Camp)
                .Include(s => s.MedicalRecords.OrderByDescending(m => m.TreatmentDate))
                .Include(s => s.CampMovements.OrderByDescending(m => m.MovementDate))
             */
            var sheep = _sheepService.GetAllSheep().FirstOrDefault(s => s.SheepID == id);

            if (sheep == null)
            {
                return NotFound();
             }
            //sheep.MedicalRecords = _medicalRecordService.GetBySheepId(id);
            //sheep.CampMovements = _campMovementService.GetBySheepId(id);

            return View(sheep);
        }
        public IActionResult RemoveSheep(int id)
        {
            try
            {
                _sheepService.DeleteSheep(id);
                return RedirectToAction("    ", "Management");

            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Sheep_Edit_Details(int id)
        {
            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            ViewBag.SheepID = id;
            return View();
        }

        [HttpPost]
        public IActionResult UpdateSheep(int sheepID, string Breed, int Camp, string Gender, DateOnly BirthDate, string? Notes, IFormFile? Photo, decimal Price)
        {
            Sheep newSheep = new Sheep();
            newSheep.SheepID = sheepID;
            newSheep.Breed = Breed;
            newSheep.CampId = Camp;
            newSheep.Gender = Gender;
            newSheep.BirthDate = BirthDate;
            newSheep.Price = Price;

            if (Photo != null)
            {
                // Save photo in database
                // Genererate and get photo ID
            }
            var currentSheep = _sheepService.getSheepByID(newSheep.SheepID);
            var newSheepList = new List<Sheep> { newSheep };
            var mergedSheepList = _sheepService.FillVoidSheppFields(currentSheep, newSheepList);

            _sheepService.UpdateSheep(mergedSheepList.First());


            
            return RedirectToAction("Sheep");
        }

        [HttpPost]
        public IActionResult SheepBulkActions(string action, string reason, List<int> selectedSheepID)
        {
            if(selectedSheepID == null || selectedSheepID.Count == 0)
            {
                RedirectToAction("Sheep");
            }
 
            _sheepService.SheepBulkActions(action, reason, selectedSheepID);
            return RedirectToAction("Sheep");
        }
        #endregion

        #region Cow Management
        public async Task<IActionResult> Cows()
        {
            var cows = await _context.Cows
                .Include(c => c.Camp)
                .Where(c => c.IsActive)
                .ToListAsync();

            return View(cows);
        }

        public IActionResult AddCow()
        {
            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCow(Cow cow)
        {
            if (ModelState.IsValid)
            {
                cow.CreatedAt = DateTime.UtcNow;
                cow.IsActive = true;
                _context.Cows.Add(cow);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Cows));
            }

            ViewBag.Camps = _context.Camps.OrderBy(c => c.CampNumber).ToList();
            return View(cow);
        }

        public async Task<IActionResult> CowDetails(int id)
        {
            var cow = await _context.Cows
                .Include(c => c.Camp)
                .Include(c => c.MedicalRecords.OrderByDescending(m => m.TreatmentDate))
                .Include(c => c.CampMovements.OrderByDescending(m => m.MovementDate))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cow == null)
            {
                return NotFound();
            }

            return View(cow);
        }
        #endregion

        #region Camp Management
        public async Task<IActionResult> Camps()
        {
            var camps = await _context.Camps
                .Include(c => c.Sheep)
                .Include(c => c.Cows)
                .OrderBy(c => c.CampNumber)
                .ToListAsync();

            return View(camps);
        }

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

            return View(camp);
        }

        [HttpPost]
        public async Task<IActionResult> MoveAnimal(int animalId, string animalType, int fromCampId, int toCampId)
        {
            var movement = new CampMovement
            {
                AnimalId = animalId,
                AnimalType = animalType,
                FromCampId = fromCampId,
                ToCampId = toCampId,
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
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
            return RedirectToAction(nameof(CampDetails), new { id = toCampId });
        }
        #endregion

        #region Staff Management
        public async Task<IActionResult> Staff()
        {
            var staff = await _context.Staff
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(staff);
        }

        public IActionResult AddStaff()
        {
            return View();
        }

        [HttpPost]
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
        public async Task<IActionResult> Tasks()
        {
            var tasks = await _context.FarmTasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tasks);
        }

        public IActionResult CreateTask()
        {
            ViewBag.Staff = _context.Staff.Where(s => s.IsActive).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(FarmTask task)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                task.CreatedById = int.Parse(userId!);
                task.CreatedAt = DateTime.UtcNow;
                task.Status = "Pending";

                _context.FarmTasks.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Tasks));
            }

            ViewBag.Staff = _context.Staff.Where(s => s.IsActive).ToList();
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            var task = await _context.FarmTasks.FindAsync(taskId);
            if (task != null)
            {
                task.Status = "Completed";
                task.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Tasks));
        }
        #endregion

        #region Water/Rainfall Management
        public async Task<IActionResult> Water()
        {
            var rainfallRecords = await _context.RainfallRecords
                .Include(r => r.Camp)
                .OrderByDescending(r => r.RainfallDate)
                .ToListAsync();

            return View(rainfallRecords);
        }

        [HttpPost]
        public async Task<IActionResult> AddRainfall(int campId, double amountMl)
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
        #endregion

        #region Assets & Finance
        public async Task<IActionResult> Assets()
        {
            var assets = await _context.Assets
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return View(assets);
        }

        public async Task<IActionResult> Finance()
        {
            var financialRecords = await _context.FinancialRecords
                .OrderByDescending(f => f.TransactionDate)
                .ToListAsync();

            return View(financialRecords);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public IActionResult Notes(string? category)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNote([Bind("Title,Content,Category")] Note input)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                // Keep employees on dashboard when invalid
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
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteNote(int id)
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
        #endregion

        // Livestock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLivestockToCamp(int campId, string animalType, string Breed, DateOnly BirthDate, string Gender, decimal Price, string? EarTag, bool? IsPregnant, DateTime? ExpectedCalvingDate, string? Notes)
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

        // Chat/Staff Communication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string content, int? recipientId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["MessageError"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Tasks));
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            int senderId;
            if (!int.TryParse(userIdStr, out senderId))
            {
                senderId = await _context.Staff.Where(s => s.IsActive).Select(s => s.Id).FirstOrDefaultAsync();
            }

            if (senderId == 0)
            {
                TempData["MessageError"] = "No active staff found to send message.";
                return RedirectToAction(nameof(Tasks));
            }

            var message = new LiveStock.Core.Models.Message
            {
                SenderId = senderId,
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
        #endregion
    }
}