using System;
using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class PaymentValidate
    {
        [Required(ErrorMessage = "User is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Payment amount is required")]
        public int PaymentAmountId { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public int PaymentMethodId { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        [RegularExpression("^(Pending|Paid|Rejected)$",
            ErrorMessage = "Payment status must be Pending, Paid, or Rejected")]
        public string PaymentStatus { get; set; } = "Pending";

        [Required(ErrorMessage = "Transaction ID is required")]
        [StringLength(100)]
        [RegularExpression(@"^[A-Za-z]{3}-\d{5,6}$",
            ErrorMessage = "Format: 3 letters, hyphen, 5–6 digits (e.g., ABC-12345)")]
        public string TransactionId { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        public DateTime PaymentDate { get; set; }
  
        [StringLength(250)]
        public string? PaymentScreenshot { get; set; }
    }
}
