using LiveStock.Core.Models;

namespace LiveStock.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalSheep { get; set; }
        public int TotalCows { get; set; }
        public int TotalStaff { get; set; }
        public int PendingTasks { get; set; }
        public List<RainfallRecord> RecentRainfall { get; set; } = new List<RainfallRecord>();
    }
} 