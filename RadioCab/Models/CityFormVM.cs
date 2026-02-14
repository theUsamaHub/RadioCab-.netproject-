using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class CityFormVM
    {
        public int CityId { get; set; }

        [Required(ErrorMessage = "City name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "City name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "City name can only contain letters and spaces")]
        public string CityName { get; set; } = null!;

        [Required(ErrorMessage = "Zip code is required")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "Zip code must be between 3 and 10 characters")]
        [RegularExpression(@"^[0-9A-Za-z\-]+$", ErrorMessage = "Zip code format is invalid")]
        public string ZipCode { get; set; } = null!;

    }
}
