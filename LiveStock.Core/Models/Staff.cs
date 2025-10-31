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
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }
        
        [Required]
        [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters")]
        [RegularExpression(@"^(Farmer|Staff)$", ErrorMessage = "Role must be either 'Farmer' or 'Staff'")]
        public string Role { get; set; } = string.Empty; // "Farmer", "Staff"
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public virtual ICollection<FarmTask> AssignedTasks { get; set; } = new List<FarmTask>();
        
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}