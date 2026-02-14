using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("MembershipFeature")]
[Index("MembershipId", "FeatureId", Name = "UQ__Membersh__1A85B6C4EED665E2", IsUnique = true)]
public partial class MembershipFeature
{
    [Key]
    public int MembershipFeatureId { get; set; }

    public int MembershipId { get; set; }

    public int FeatureId { get; set; }

    public int? MaxAmount { get; set; }

    public bool IsEnabled { get; set; }

    [ForeignKey("FeatureId")]
    [InverseProperty("MembershipFeatures")]
    public virtual Feature Feature { get; set; } = null!;

    [ForeignKey("MembershipId")]
    [InverseProperty("MembershipFeatures")]
    public virtual Membership Membership { get; set; } = null!;
}
