using LiveStock.Core.Models;
using LiveStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveStock.Web.Service
{
    public interface INoteService
    {
        Task<List<Note>> GetUserNotesAsync(int userId, string? category = null, int? take = null);
        Task<Note?> GetNoteByIdAsync(int id, int userId);
        Task<Note?> CreateNoteAsync(int userId, string title, string content, string category, DateTime? createdAt = null);
        Task<bool> DeleteNoteAsync(int id, int userId);
    }

    public class noteService : INoteService
    {
        private readonly LiveStockDbContext _context;

        public noteService(LiveStockDbContext context)
        {
            _context = context;
        }

        public async Task<List<Note>> GetUserNotesAsync(int userId, string? category = null, int? take = null)
        {
            var query = _context.Notes
                .Where(n => n.CreatedByUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(n => n.Category == category);
            }

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Note?> GetNoteByIdAsync(int id, int userId)
        {
            return await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.CreatedByUserId == userId);
        }

        public async Task<Note?> CreateNoteAsync(int userId, string title, string content, string category, DateTime? createdAt = null)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(category))
            {
                return null;
            }

            // Trim and enforce reasonable limits (mirrors model attributes)
            title = title.Trim();
            content = content.Trim();
            category = category.Trim();

            var note = new Note
            {
                Title = title,
                Content = content,
                Category = category,
                CreatedByUserId = userId,
                CreatedAt = createdAt ?? DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<bool> DeleteNoteAsync(int id, int userId)
        {
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.CreatedByUserId == userId);
            if (note == null)
            {
                return false;
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}