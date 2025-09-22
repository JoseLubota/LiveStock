using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public abstract class Animal
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Breed { get; set; } = string.Empty;
        
        [Required]
        public int Age { get; set; }
        
        [Required]
        public int CampId { get; set; }
        
        public virtual Camp Camp { get; set; } = null!;
        
        [Required]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? Notes { get; set; }
        
        public string? PhotoUrl { get; set; }
    }
} 