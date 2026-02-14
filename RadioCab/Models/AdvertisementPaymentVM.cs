using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class AdvertisementCreateVM
    {
        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Invalid title")]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(AdvertisementCreateVM), nameof(ValidateStartDate))]
        public DateTime StartDate { get; set; }

        public static ValidationResult ValidateStartDate(DateTime date, ValidationContext context)
        {
            if (date.Date < DateTime.Today)
                return new ValidationResult("Past dates not allowed");

            return ValidationResult.Success;
        }
    }

    // Keep the original for payment processing
    public class AdvertisementPaymentVM
    {
        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Invalid title")]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(AdvertisementPaymentVM), nameof(ValidateStartDate))]
        public DateTime StartDate { get; set; }

        [Required]
        public int PaymentAmountId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Invalid Transaction ID")]
        public string TransactionId { get; set; }

        public static ValidationResult ValidateStartDate(DateTime date, ValidationContext context)
        {
            if (date.Date < DateTime.Today)
                return new ValidationResult("Past dates not allowed");

            return ValidationResult.Success;
        }
    }
}
