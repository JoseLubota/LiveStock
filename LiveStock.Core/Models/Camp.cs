using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class Camp
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int CampNumber { get; set; }
        
        public double Hectares { get; set; }
        
        public string? Description { get; set; }
        
        public virtual ICollection<Sheep> Sheep { get; set; } = new List<Sheep>();
        
        public virtual ICollection<Cow> Cows { get; set; } = new List<Cow>();
        
        public virtual ICollection<CampMovement> CampMovements { get; set; } = new List<CampMovement>();
        
        public virtual ICollection<RainfallRecord> RainfallRecords { get; set; } = new List<RainfallRecord>();
    }
} 