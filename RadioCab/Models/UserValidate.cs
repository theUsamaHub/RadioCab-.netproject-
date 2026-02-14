using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class UserValidate
    {


        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        [RegularExpression(@"^[A-Za-z ]{3,100}$", ErrorMessage = "Only letters and spaces allowed")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Password must contain Upper, Lower, Number & Symbol")]
        public string Password { get; set; } = null!;

        [StringLength(20)]
        [RegularExpression(@"^[0-9+\- ]{7,20}$", ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

    }
}
