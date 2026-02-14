using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("Advertisement")]
public partial class Advertisement
{
    [Key]
    public int AdvertisementId { get; set; }

    [StringLength(20)]
    public string AdvertiserType { get; set; } = null!;

    public int AdvertiserId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    [StringLength(250)]
    public string? AdImage { get; set; }

    public int? PaymentId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [StringLength(20)]
    public string ApprovalStatus { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("PaymentId")]
    [InverseProperty("Advertisements")]
    public virtual Payment? Payment { get; set; }
}
