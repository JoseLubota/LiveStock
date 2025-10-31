using System.ComponentModel.DataAnnotations;
using LiveStock.Core.Validation;

namespace LiveStock.Core.Models
{
    public class Message
    {
        public int Id { get; set; }
        
        [Required]
        public int SenderId { get; set; }
        
        public virtual Staff Sender { get; set; } = null!;
        
        public int? RecipientId { get; set; } // null for broadcast messages
        
        public virtual Staff? Recipient { get; set; }
        
        [Required]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 2000 characters")]
        [SafeString(ErrorMessage = "Message content contains potentially unsafe content")]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        [DataType(DataType.DateTime)]
        public DateTime? ReadAt { get; set; }
        
        public bool IsBroadcast { get; set; } = false; // true if sent to all staff
        
        [StringLength(500, ErrorMessage = "Attachment URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [AllowedFileExtensions(".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".txt", ErrorMessage = "Attachment must be a valid file type")]
        public string? AttachmentUrl { get; set; }
    }
}