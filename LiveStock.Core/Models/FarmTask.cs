using System.ComponentModel.DataAnnotations;
using LiveStock.Core.Validation;

namespace LiveStock.Core.Models
{
    public class FarmTask
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public int AssignedToId { get; set; }
        
        public virtual Staff? AssignedTo { get; set; }
        
        [Required]
        [StringLength(20, ErrorMessage = "Importance cannot exceed 20 characters")]
        [RegularExpression(@"^(Low|Medium|High|Critical)$", ErrorMessage = "Importance must be Low, Medium, High, or Critical")]
        public string Importance { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression(@"^(Pending|In Progress|Completed)$", ErrorMessage = "Status must be Pending, In Progress, or Completed")]
        public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Completed"
        
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [SafeString(ErrorMessage = "Notes contain potentially unsafe content")]
        public string? Notes { get; set; }
        
        [StringLength(500, ErrorMessage = "Photo URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [AllowedFileExtensions(".jpg", ".jpeg", ".png", ".gif", ".webp", ErrorMessage = "Photo must be a valid image file")]
        public string? PhotoUrl { get; set; } // For task completion photos
        
        public int CreatedById { get; set; }
        
        public virtual Staff? CreatedBy { get; set; }
    }
}