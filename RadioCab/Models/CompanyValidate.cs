using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class CompanyValidate
    {
        public int CompanyId { get; set; }
        [Required(ErrorMessage = "Company Name is required")]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "Company Name only contains Letters")]
        public string CompanyName { get; set; } = null!;
        [Required(ErrorMessage = "Contact person's designation is required")]
        [RegularExpression(@"^[A-Za-z]+(?:[A-Za-z .&/-]*[A-Za-z])$", ErrorMessage = "Please enter a valid designation")]
        public string Designation { get; set; }
        [Required(ErrorMessage = "Company address is required")]
        [RegularExpression(@"^[A-Za-z0-9 #,./-]+$", ErrorMessage = "Please enter a valid address")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Fax number is required")]
        [RegularExpression(@"^[0-9]{6,15}$", ErrorMessage = "Fax number only contains numbers.")]
        public string FaxNumber { get; set; }
        [Required(ErrorMessage = "City is required")]

        public int CityId { get; set; }
        [Required(ErrorMessage = "Company Description is required")]
        public string Description { get; set; }
        [Required]
        public int MembershipId { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? ContactPerson { get; set; }


        public string? CompanyLogo { get; set; }


        public string? FbrCertificate { get; set; }


        public string? BusinessLicense { get; set; }

    }
}
