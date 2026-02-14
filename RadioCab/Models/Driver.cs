using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("Driver")]
public partial class Driver
{
    [Key]
    public int DriverId { get; set; }

    public int UserId { get; set; }

    [StringLength(150)]
    public string DriverName { get; set; } = null!;

    [StringLength(250)]
    public string? Address { get; set; }

    public int CityId { get; set; }

    [StringLength(20)]
    public string? Telephone { get; set; }

    [StringLength(50)]
    public string? Cnic { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? Experience { get; set; }

    public string? Description { get; set; }

    [StringLength(250)]
    public string? DriverPhoto { get; set; }

    [StringLength(50)]
    public string? DrivingLicenseNumber { get; set; }

    [StringLength(250)]
    public string? DrivingLicenseFile { get; set; }

    [StringLength(250)]
    public string? VehicleInfo { get; set; }

    public int MembershipId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string RegisterationStatus { get; set; } = null!;

    [ForeignKey("CityId")]
    [InverseProperty("Drivers")]
    public virtual City City { get; set; } = null!;

    [InverseProperty("Driver")]
    public virtual ICollection<DriverFeedback> DriverFeedbacks { get; set; } = new List<DriverFeedback>();

    [InverseProperty("Driver")]
    public virtual ICollection<DriverService> DriverServices { get; set; } = new List<DriverService>();

    [ForeignKey("MembershipId")]
    [InverseProperty("Drivers")]
    public virtual Membership Membership { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Drivers")]
    public virtual User User { get; set; } = null!;
}
