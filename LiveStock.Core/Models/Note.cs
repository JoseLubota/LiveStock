using System.ComponentModel.DataAnnotations;
using LiveStock.Core.Validation;

namespace LiveStock.Core.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [SafeString(ErrorMessage = "Title contains potentially unsafe content")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        [SafeString(ErrorMessage = "Content contains potentially unsafe content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [RegularExpression("^(Sheep|Cow|Tasks|Camps|Finance|Staff)$", ErrorMessage = "Invalid category")] 
        public string Category { get; set; } = string.Empty; // "Sheep", "Cow", "Tasks", "Camps", "Finance", "Staff"

        [Required]
        public int CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}