using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class RainfallRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int CampId { get; set; }
        
        public virtual Camp Camp { get; set; } = null!;
        
        [Required]
        public DateTime RainfallDate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public double AmountMl { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 