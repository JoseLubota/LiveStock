namespace LiveStock.Core.Models
{
    public class Sheep : Animal
    {
        public string? TagNumber { get; set; }
        
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        
        public virtual ICollection<CampMovement> CampMovements { get; set; } = new List<CampMovement>();
    }
} 