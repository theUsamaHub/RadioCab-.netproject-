using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("Feature")]
[Index("FeatureKey", Name = "UQ__Feature__FCEFA3304746D133", IsUnique = true)]
public partial class Feature
{
    [Key]
    public int FeatureId { get; set; }

    [StringLength(50)]
    public string FeatureKey { get; set; } = null!;

    [StringLength(100)]
    public string FeatureName { get; set; } = null!;

    [InverseProperty("Feature")]
    public virtual ICollection<MembershipFeature> MembershipFeatures { get; set; } = new List<MembershipFeature>();
}
