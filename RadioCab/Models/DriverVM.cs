using RadioCab.Models;
namespace RadioCab.Models.ViewModels
{
    public class DriverVM
    {
        public Driver Driver { get; set; }
        public double AverageRating { get; set; }
        public int FeedbackCount { get; set; }
        public int ServicesCount { get; set; } // Add this
    }
}