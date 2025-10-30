using LiveStock.Core.Models;
using LiveStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveStock.Web.Service
{
    public interface IStaffService
    {
        Task<IEnumerable<Staff>> GetAllStaffAsync();
        Task<IEnumerable<Staff>> GetActiveStaffAsync();
        Task<Staff?> GetStaffByIdAsync(int id);
        Task<bool> AddStaffAsync(Staff staff);
        Task<bool> UpdateStaffAsync(Staff staff);
        Task<bool> RemoveStaffAsync(int id);
        Task<int> GetTotalStaffCount();
    }

    public class staffService : IStaffService
    {
        private readonly LiveStockDbContext _context;
        private readonly ILogger<staffService> _logger;

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes staff service with EF DbContext"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public staffService(LiveStockDbContext context, ILogger<staffService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Returns all staff ordered by name"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IEnumerable<Staff>> GetAllStaffAsync()
        {
            try
            {
                return await _context.Staff
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllStaffAsync failed");
                return Enumerable.Empty<Staff>();
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Returns only active staff ordered by name"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<IEnumerable<Staff>> GetActiveStaffAsync()
        {
            try
            {
                return await _context.Staff
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetActiveStaffAsync failed");
                return Enumerable.Empty<Staff>();
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Gets a staff member by id, including assigned tasks"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<Staff?> GetStaffByIdAsync(int id)
        {
            try
            {
                return await _context.Staff
                    .Include(s => s.AssignedTasks)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStaffByIdAsync failed for ID {StaffId}", id);
                return null;
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Adds a new staff member after verifying employee ID is unique"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<bool> AddStaffAsync(Staff staff)
        {
            try
            {
                var existingStaff = await _context.Staff
                    .FirstOrDefaultAsync(s => s.EmployeeId == staff.EmployeeId);
                
                if (existingStaff != null)
                {
                    return false;
                }

                staff.CreatedAt = DateTime.UtcNow;
                staff.IsActive = true;

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddStaffAsync failed for EmployeeId {EmployeeId}", staff?.EmployeeId);
                return false;
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates staff details and sets updated timestamp"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<bool> UpdateStaffAsync(Staff staff)
        {
            try
            {
                var existingStaff = await _context.Staff.FindAsync(staff.Id);
                if (existingStaff == null)
                {
                    return false;
                }

                existingStaff.Name = staff.Name;
                existingStaff.EmployeeId = staff.EmployeeId;
                existingStaff.PhoneNumber = staff.PhoneNumber;
                existingStaff.Email = staff.Email;
                existingStaff.Role = staff.Role;
                existingStaff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateStaffAsync failed for ID {StaffId}", staff?.Id);
                return false;
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Soft deletes a staff member by marking them inactive"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<bool> RemoveStaffAsync(int id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null)
                {
                    return false;
                }

                staff.IsActive = false;
                staff.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveStaffAsync failed for ID {StaffId}", id);
                return false;
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Counts total active staff members"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<int> GetTotalStaffCount()
        {
            try
            {
                return await _context.Staff.CountAsync(s => s.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTotalStaffCount failed");
                return 0;
            }
        }
    }
}
