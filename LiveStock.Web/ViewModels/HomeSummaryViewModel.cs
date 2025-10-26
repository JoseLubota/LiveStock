using System.Collections.Generic;

namespace LiveStock.Web.ViewModels
{
    public class HomeSummaryViewModel
    {
        // Key metrics
        public int LivestockCount { get; set; }
        public int ActiveStaffCount { get; set; }
        public int CampCount { get; set; }
        public double RainfallThisWeekMl { get; set; }
        public int OpenTaskCount { get; set; }
        public decimal NetProfit { get; set; }

        // Today highlights
        public List<string> TodayHighlights { get; set; } = new List<string>();
        public List<(string Title, string Category, string When)> RecentNotes { get; set; } = new List<(string, string, string)>();

        // Role context
        public string? UserRole { get; set; } // "Admin" or "Employee" or null
        public string? UserName { get; set; }
        public bool IsAuthenticated { get; set; }

        // Announcements (static/demo for now)
        public List<string> Announcements { get; set; } = new List<string>();
    }
}