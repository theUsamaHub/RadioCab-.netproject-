using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("CompanyVacancy")]
public partial class CompanyVacancy
{
    [Key]
    public int VacancyId { get; set; }

    public int CompanyId { get; set; }

    [StringLength(150)]
    public string JobTitle { get; set; } = null!;

    public string JobDescription { get; set; } = null!;

    [StringLength(50)]
    public string JobType { get; set; } = null!;

    [StringLength(50)]
    public string? RequiredExperience { get; set; }

    [StringLength(50)]
    public string? Salary { get; set; }

    [StringLength(150)]
    public string? Location { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ApprovalStatus { get; set; } = null!;

    public string? AdminRemarks { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("CompanyVacancies")]
    public virtual Company Company { get; set; } = null!;

    [InverseProperty("Vacancy")]
    public virtual ICollection<VacancyApplication> VacancyApplications { get; set; } = new List<VacancyApplication>();
}
