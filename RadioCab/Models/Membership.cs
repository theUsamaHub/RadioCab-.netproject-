using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("Membership")]
public partial class Membership
{
    [Key]
    public int MembershipId { get; set; }

    [StringLength(50)]
    public string MembershipName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Membership")]
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    [InverseProperty("Membership")]
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    [InverseProperty("Membership")]
    public virtual ICollection<MembershipFeature> MembershipFeatures { get; set; } = new List<MembershipFeature>();

    [InverseProperty("Membership")]
    public virtual ICollection<PaymentAmount> PaymentAmounts { get; set; } = new List<PaymentAmount>();
}
