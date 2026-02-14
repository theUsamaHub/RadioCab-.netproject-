using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class MembershipFeatureValidate
    {
        public int MembershipFeatureId { get; set; }

        [Required(ErrorMessage = "Membership is required.")]
        public int MembershipId { get; set; }

        [Required(ErrorMessage = "Feature is required.")]
        public int FeatureId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max Amount must be zero or a positive number.")]
        public int? MaxAmount { get; set; }

        public bool IsEnabled { get; set; }
    }
}
