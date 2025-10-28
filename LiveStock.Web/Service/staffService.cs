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

        public staffService(LiveStockDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Staff>> GetAllStaffAsync()
        {
            return await _context.Staff
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Staff>> GetActiveStaffAsync()
        {
            return await _context.Staff
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Staff?> GetStaffByIdAsync(int id)
        {
            return await _context.Staff
                .Include(s => s.AssignedTasks)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> AddStaffAsync(Staff staff)
        {
            try
            {
                // Check if employee ID already exists
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
            catch
            {
                return false;
            }
        }

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
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveStaffAsync(int id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null)
                {
                    return false;
                }

                // Soft delete - just mark as inactive
                staff.IsActive = false;
                staff.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetTotalStaffCount()
        {
            return await _context.Staff.CountAsync(s => s.IsActive);
        }
    }
}