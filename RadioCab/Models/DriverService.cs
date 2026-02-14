using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Index("DriverId", "ServiceId", Name = "UQ_Driver_Service", IsUnique = true)]
public partial class DriverService
{
    [Key]
    public int DriverServiceId { get; set; }

    public int DriverId { get; set; }

    public int ServiceId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("DriverServices")]
    public virtual Driver Driver { get; set; } = null!;

    [ForeignKey("ServiceId")]
    [InverseProperty("DriverServices")]
    public virtual Service Service { get; set; } = null!;
}
