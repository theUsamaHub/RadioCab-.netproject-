using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class PaymentMethodValidate
    {
        public int PaymentMethodId { get; set; }

        [Required(ErrorMessage = "Payment method name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Method name must be between 3 and 50 characters.")]
        public string MethodName { get; set; } = string.Empty;

        public bool? IsActive { get; set; }

    }
}
