using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class MembershipValidate
    {
        public int MembershipId { get; set; }

        [Required(ErrorMessage = "Membership name is required")]
        [StringLength(150, ErrorMessage = "Membership name cannot exceed 150 characters")]
        public string MembershipName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public bool? IsActive { get; set; }
    }
}
