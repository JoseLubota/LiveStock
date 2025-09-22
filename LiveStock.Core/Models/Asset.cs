using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class Asset
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty; // "Feed", "Fencing", "Vehicles", "Cattle Lists"
        
        [Required]
        public string Type { get; set; } = string.Empty; // Specific type within category
        
        public string? Description { get; set; }
        
        public int? Quantity { get; set; }
        
        public string? Unit { get; set; } // kg, pieces, liters, etc.
        
        public decimal? PurchasePrice { get; set; }
        
        public DateTime? PurchaseDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public string Status { get; set; } = "Active"; // "Active", "Maintenance", "Retired"
        
        public string? Location { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 