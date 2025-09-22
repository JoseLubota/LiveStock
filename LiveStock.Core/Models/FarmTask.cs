using System.ComponentModel.DataAnnotations;

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
        
        public virtual Staff AssignedTo { get; set; } = null!;
        
        [Required]
        public string Importance { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        
        [Required]
        public DateTime DueDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Completed"
        
        public string? Notes { get; set; }
        
        public string? PhotoUrl { get; set; } // For task completion photos
        
        public int CreatedById { get; set; }
        
        public virtual Staff CreatedBy { get; set; } = null!;
    }
} 