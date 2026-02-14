using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadioCab.Models
{
    public class PaymentAmountValidate
    {
        public int PaymentAmountId { get; set; }

        [Required(ErrorMessage = "Membership is required")]
        public int MembershipId { get; set; }

        [Required(ErrorMessage = "Entity type is required")]
        [StringLength(50, ErrorMessage = "Entity type cannot exceed 50 characters")]
        public string EntityType { get; set; } = null!;

        [Required(ErrorMessage = "Payment type is required")]
        [StringLength(50, ErrorMessage = "Payment type cannot exceed 50 characters")]
        public string PaymentType { get; set; } = null!;

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 60, ErrorMessage = "Duration must be between 1 and 60 months")]
        public int DurationInMonths { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 10000000, ErrorMessage = "Amount must be greater than 0")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
