using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("DriverFeedback")]
public partial class DriverFeedback
{
    [Key]
    public int FeedbackId { get; set; }

    public int DriverId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("DriverFeedbacks")]
    public virtual Driver Driver { get; set; } = null!;
}
