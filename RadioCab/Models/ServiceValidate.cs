using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class ServiceValidate
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Service name must be at least 3 characters long")]
        public string ServiceName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? ServiceDescription { get; set; }

        public bool IsForDriver { get; set; }
       
        public bool IsForCompany { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
