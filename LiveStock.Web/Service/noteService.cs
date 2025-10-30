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
        private readonly ILogger<noteService> _logger;

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes note service with EF DbContext"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public noteService(LiveStockDbContext context, ILogger<noteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Returns notes for a user, filtered by category and limited by take"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<List<Note>> GetUserNotesAsync(int userId, string? category = null, int? take = null)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserNotesAsync failed for UserId {UserId}", userId);
                return new List<Note>();
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Gets a single note by id that belongs to the given user"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<Note?> GetNoteByIdAsync(int id, int userId)
        {
            try
            {
                return await _context.Notes
                    .FirstOrDefaultAsync(n => n.Id == id && n.CreatedByUserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetNoteByIdAsync failed for NoteId {NoteId} and UserId {UserId}", id, userId);
                return null;
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Creates a new note for the user and saves it to the database"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<Note?> CreateNoteAsync(int userId, string title, string content, string category, DateTime? createdAt = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(category))
                {
                    return null;
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateNoteAsync failed for UserId {UserId}", userId);
                return null;
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes the user's note by id and persists the change"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<bool> DeleteNoteAsync(int id, int userId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteNoteAsync failed for NoteId {NoteId} and UserId {UserId}", id, userId);
                return false;
            }
        }
    }
}
