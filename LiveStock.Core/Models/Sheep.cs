namespace LiveStock.Core.Models
{
    public class Sheep : Animal
    {

       public int SheepID { set; get; }
        public DateOnly BirthDate { set; get; }
        public int? PhotoID { set; get; }
        
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        
        public virtual ICollection<CampMovement> CampMovements { get; set; } = new List<CampMovement>();
    }
} 