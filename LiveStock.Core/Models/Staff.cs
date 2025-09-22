using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Models
{
    public class Staff
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string EmployeeId { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [Required]
        public string Role { get; set; } = string.Empty; // "Farmer", "Staff"
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public virtual ICollection<FarmTask> AssignedTasks { get; set; } = new List<FarmTask>();
        
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
} 