using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("VacancyApplication")]
public partial class VacancyApplication
{
    [Key]
    public int ApplicationId { get; set; }

    public int VacancyId { get; set; }

    [StringLength(150)]
    public string ApplicantName { get; set; } = null!;

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string MobileNo { get; set; } = null!;

    [StringLength(250)]
    public string CvFile { get; set; } = null!;

    public string? CoverLetter { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [ForeignKey("VacancyId")]
    [InverseProperty("VacancyApplications")]
    public virtual CompanyVacancy Vacancy { get; set; } = null!;
}
