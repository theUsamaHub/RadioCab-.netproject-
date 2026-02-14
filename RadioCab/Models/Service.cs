using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Index("ServiceName", Name = "UQ__Services__A42B5F9973C4124C", IsUnique = true)]
public partial class Service
{
    [Key]
    public int ServiceId { get; set; }

    [StringLength(150)]
    public string ServiceName { get; set; } = null!;

    public string? ServiceDescription { get; set; }

    public bool IsForDriver { get; set; }

    public bool IsForCompany { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Service")]
    public virtual ICollection<CompanyService> CompanyServices { get; set; } = new List<CompanyService>();

    [InverseProperty("Service")]
    public virtual ICollection<DriverService> DriverServices { get; set; } = new List<DriverService>();
}
