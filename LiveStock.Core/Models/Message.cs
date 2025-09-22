using System.ComponentModel.DataAnnotations;

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
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadAt { get; set; }
        
        public bool IsBroadcast { get; set; } = false; // true if sent to all staff
        
        public string? AttachmentUrl { get; set; }
    }
} 