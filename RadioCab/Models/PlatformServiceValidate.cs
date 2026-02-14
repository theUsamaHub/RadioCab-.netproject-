using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class PlatformServiceValidate
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(150, ErrorMessage = "Service name cannot exceed 150 characters")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string ServiceDescription { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
