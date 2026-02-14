using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

public partial class PlatformService
{
    [Key]
    public int ServiceId { get; set; }

    [StringLength(150)]
    public string ServiceName { get; set; } = null!;

    public string? ServiceDescription { get; set; }

    public bool? isActive { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ApprovalStatus { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
