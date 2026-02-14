using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class AdvertisementListVM
    {
        public int AdvertisementId { get; set; }
        
        [Required]
        [StringLength(150)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string? AdImage { get; set; }
        
        [StringLength(20)]
        public string ApprovalStatus { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string PaymentStatus { get; set; }
        
        public bool CanEdit { get; set; }
        
        public int ActiveAdCount { get; set; }
    }
}
