using Microsoft.EntityFrameworkCore;
using RadioCab.Validations;
using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class CompanyVacancyValidate
    {
        public int VacancyId { get; set; }

        public int CompanyId { get; set; }

        [StringLength(150)]
        [Required(ErrorMessage = "Job Title is required.")]
        [RegularExpression(@"^[a-zA-Z\s\.,'-]{1,150}$", ErrorMessage = "Job Title can only contain letters, spaces, and basic punctuation (.,'-).")]
        public string JobTitle { get; set; } = null!;
        [Required(ErrorMessage = "Job Description is required.")]
        public string JobDescription { get; set; } = null!;
        [Required(ErrorMessage = "Job Type is required.")]
        [StringLength(50)]
        public string JobType { get; set; } = null!;

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-]{1,50}$",ErrorMessage = "Required Experience can contain only letters, numbers, spaces, and hyphens (e.g., 1-3 years).")]
        public string? RequiredExperience { get; set; }
        [Required(ErrorMessage = "Salary Range is required.")]
        [StringLength(50)]
        [SalaryRange]
        public string? Salary { get; set; }
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(150)]
        [RegularExpression(@"^[a-zA-Z0-9\s\.,'-]{1,150}$", ErrorMessage = "Location can contain only letters, numbers, spaces, and basic punctuation (.,-').")]
        public string Location { get; set; }


    }
}
