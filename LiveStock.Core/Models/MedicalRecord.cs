using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int AnimalId { get; set; }
        
        [Required]
        public string AnimalType { get; set; } = string.Empty; // "Sheep" or "Cow"
        
        [Required]
        [StringLength(200)]
        public string Treatment { get; set; } = string.Empty;
        
        [Required]
        public DateTime TreatmentDate { get; set; }
        
        public string? Notes { get; set; }
        
        public string? Veterinarian { get; set; }
        
        public decimal? Cost { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 