using LiveStock.Core.Models;

namespace LiveStock.Web.ViewModels
{
    public class WaterViewModel
    {
        public List<Camp> Camps { get; set; } = new List<Camp>();
        public List<RainfallRecord> RainfallRecords { get; set; } = new List<RainfallRecord>();
    }
}