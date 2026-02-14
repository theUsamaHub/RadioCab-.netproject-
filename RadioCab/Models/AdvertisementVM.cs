
using RadioCab.Models;
using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models.ViewModels
{

    public class AdvertisementVM
    {
        public int AdvertisementId { get; set; }
        public string AdvertiserType { get; set; } // "Company" or "Driver"
        public int AdvertiserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AdImage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // For navigation
        public string AdvertiserName { get; set; }
        public string CityName { get; set; }
        public string ProfileImage { get; set; }
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }

        // Helper property for UI
        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
        public int DaysRemaining => (EndDate - DateTime.Now).Days;
    }
}