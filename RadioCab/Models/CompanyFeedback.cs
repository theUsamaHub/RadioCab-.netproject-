using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("CompanyFeedback")]
public partial class CompanyFeedback
{
    [Key]
    public int FeedbackId { get; set; }

    public int CompanyId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("CompanyFeedbacks")]
    public virtual Company Company { get; set; } = null!;
}
