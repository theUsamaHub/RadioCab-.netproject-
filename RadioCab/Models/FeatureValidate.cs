using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class FeatureValidate
    {
        public int FeatureId { get; set; }

        [Required (ErrorMessage = "Feature key is required")]
        [StringLength(50, ErrorMessage = "Feature Key cannot exceed 50 characters")]
        public string FeatureKey { get; set; } = null!;

        [Required(ErrorMessage = "Feature Name is required")]
        [StringLength(100, ErrorMessage = "Feature Name cannot exceed 100 character")]
        public string FeatureName { get; set; } = null!;
    }
}
