using System.ComponentModel.DataAnnotations;

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

    // Changed from string to IFormFile for file upload
    public IFormFile? CoverLetterFile { get; set; }
}