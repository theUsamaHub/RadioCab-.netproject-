using RadioCab.Models;
using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models.ViewModels
{
    public class VacancyVM
    {
        public CompanyVacancy Vacancy { get; set; }
        public int ApplicationCount { get; set; }
        public bool IsEligibleToApply { get; set; } = true;
    }

    public class VacancyApplicationVM
    {
        public int VacancyId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string ApplicantName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Mobile number cannot exceed 20 characters")]
        public string MobileNo { get; set; } = null!;

        [Required(ErrorMessage = "CV file is required")]
        public IFormFile CvFile { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Cover letter cannot exceed 1000 characters")]
        public string? CoverLetter { get; set; }
    }
}