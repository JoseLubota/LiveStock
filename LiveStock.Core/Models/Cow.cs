namespace LiveStock.Core.Models
{
    public class Cow : Animal
    {
        public string? EarTag { get; set; }
        
        public bool IsPregnant { get; set; }
        
        public DateTime? ExpectedCalvingDate { get; set; }
        
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        
        public virtual ICollection<CampMovement> CampMovements { get; set; } = new List<CampMovement>();
    }
} 