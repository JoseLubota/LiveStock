using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class FinancialRecord
    {
        public int Id { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime TransactionDate { get; set; }
        
        public string? Category { get; set; } // "Livestock Sales", "Feed", "Veterinary", "Equipment", etc.
        
        public string? Reference { get; set; } // Invoice number, receipt number, etc.
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 