using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // "Sheep", "Cow", "Tasks", "Camps", "Finance", "Staff"

        [Required]
        public int CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}