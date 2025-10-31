using System.ComponentModel.DataAnnotations;
using LiveStock.Core.Validation;

namespace LiveStock.Core.Models
{
    public abstract class Animal
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Breed { get; set; } = string.Empty;
        
        [Required]
        [NotFutureDate(ErrorMessage = "Birth date cannot be in the future")]
        [ReasonableDateRange(50, 0, ErrorMessage = "Birth date must be within the last 50 years")]
        public DateOnly BirthDate { get; set; }
        
        [Required]
        public int CampId { get; set; }
        [Required]
        public string Status { get; set; }

        public virtual Camp Camp { get; set; } = null!;
        
        [Required]
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        [RegularExpression(@"^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'")]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 1000000, ErrorMessage = "Price must be between 0 and 1,000,000")]
        public decimal Price { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [SafeString(ErrorMessage = "Notes contain potentially unsafe content")]
        public string? Notes { get; set; }
        
        [StringLength(500, ErrorMessage = "Photo URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [AllowedFileExtensions(".jpg", ".jpeg", ".png", ".gif", ".webp", ErrorMessage = "Photo must be a valid image file")]
        public string? PhotoUrl { get; set; }
    }
}