using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

public partial class CompanyService
{
    [Key]
    public int CompanyServiceId { get; set; }

    public int CompanyId { get; set; }

    public int ServiceId { get; set; }

    public string? ServiceDescription { get; set; }

    public bool? isActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("CompanyServices")]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey("ServiceId")]
    [InverseProperty("CompanyServices")]
    public virtual Service Service { get; set; } = null!;
}
