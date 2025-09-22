using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class CampMovement
    {
        public int Id { get; set; }
        
        [Required]
        public int AnimalId { get; set; }
        
        [Required]
        public string AnimalType { get; set; } = string.Empty; // "Sheep" or "Cow"
        
        [Required]
        public int FromCampId { get; set; }
        
        public virtual Camp FromCamp { get; set; } = null!;
        
        [Required]
        public int ToCampId { get; set; }
        
        public virtual Camp ToCamp { get; set; } = null!;
        
        [Required]
        public DateTime MovementDate { get; set; }
        
        public string? Reason { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 