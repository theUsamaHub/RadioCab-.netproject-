using System.ComponentModel.DataAnnotations;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RadioCab.Models
{
    public class DriverValidate
    {
        public int DriverId { get; set; }
        [Required(ErrorMessage = "Company Name is required")]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "Company Name only contains Letters")]
        public string? DriverName { get; set; }
        [Required(ErrorMessage = "Address is reqired")]
        [RegularExpression(@"^[A-Za-z0-9 #,./-]+$", ErrorMessage = "Please enter a valid address")]
        public string Address { get; set; }

        public int CityId { get; set; }
        [Required(ErrorMessage = "CNIC is reqired")]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must be in 12345-1234567-1 format.")]
        public string Cnic { get; set; }
        public string? Telephone { get; set; }

        public string? Email { get; set; }
        [Required(ErrorMessage = "Experience Name is reqired")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Please enter Experience in valid format.")]
        public string? Experience { get; set; }
        [Required(ErrorMessage = "Description is reqired")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Please provide your vehicle’s make (e.g., Toyota, Honda).")]
        [RegularExpression(@"^[A-Za-z0-9]+([ -][A-Za-z0-9]+)*$", ErrorMessage = "Enter a valid vehicle make (letters, numbers, spaces, or hyphens only)")]

        public string VehicleMake { get; set; }
        [Required(ErrorMessage = "Please provide the vehicle model(e.g., Corolla, Civic).")]
        [RegularExpression(@"^[A-Za-z0-9]+([ -][A-Za-z0-9]+)*$", ErrorMessage = "Enter a valid vehicle model (letters, numbers, spaces, or hyphens only)")]

        public string VehicleModel { get; set; }
        [Required(ErrorMessage = "Please provide your vehicle’s license plate number.")]
        [RegularExpression(@"^[A-Z]{2,3}-\d{1,4}$", ErrorMessage = "Enter a valid license plate (e.g., LE-1234)")]
        public string VehiclePlate { get; set; }
        [Required(ErrorMessage = "Please provide the year your vehicle was manufactured.")]
        [RegularExpression(@"^(19|20)\d{2}$", ErrorMessage = "Enter a valid 4-digit year (1900–2099)")]
        public string VehicleYear { get; set; }
        [Required(ErrorMessage = "Please enter color of you vehicle.")]
        [RegularExpression(@"^[A-Za-z]+( [A-Za-z]+)*$", ErrorMessage = "Enter a valid vehicle color (letters and spaces only, e.g., Dark Blue)")]

        public string VehicleColor { get; set; }

        public string? VehicleInfo { get; set; }
        public int MembershipId { get; set; }
        
        [RegularExpression(@"^[A-Z]{1,3}-\d{6,8}[A-Z]?$", ErrorMessage = "Enter a valid driving license number (e.g., LE-1234567 or ISB-123456A)")]

        public string? DrivingLicenseNumber { get; set; }


    }
}
