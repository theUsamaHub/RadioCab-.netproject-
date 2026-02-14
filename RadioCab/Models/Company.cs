using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("Company")]
public partial class Company
{
    [Key]
    public int CompanyId { get; set; }

    public int UserId { get; set; }

    [StringLength(150)]
    public string CompanyName { get; set; } = null!;

    [StringLength(100)]
    public string ContactPerson { get; set; } = null!;

    [StringLength(100)]
    public string? Designation { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    public int CityId { get; set; }

    [StringLength(20)]
    public string? Telephone { get; set; }

    [StringLength(20)]
    public string? FaxNumber { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    public string? Description { get; set; }

    [StringLength(250)]
    public string? CompanyLogo { get; set; }

    [StringLength(250)]
    public string? FbrCertificate { get; set; }

    [StringLength(250)]
    public string? BusinessLicense { get; set; }

    public int MembershipId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string RegisterationStatus { get; set; } = null!;

    [ForeignKey("CityId")]
    [InverseProperty("Companies")]
    public virtual City City { get; set; } = null!;

    [InverseProperty("Company")]
    public virtual ICollection<CompanyFeedback> CompanyFeedbacks { get; set; } = new List<CompanyFeedback>();

    [InverseProperty("Company")]
    public virtual ICollection<CompanyService> CompanyServices { get; set; } = new List<CompanyService>();

    [InverseProperty("Company")]
    public virtual ICollection<CompanyVacancy> CompanyVacancies { get; set; } = new List<CompanyVacancy>();

    [ForeignKey("MembershipId")]
    [InverseProperty("Companies")]
    public virtual Membership Membership { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Companies")]
    public virtual User User { get; set; } = null!;
}
