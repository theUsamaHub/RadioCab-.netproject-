using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("ContactRequest")]
public partial class ContactRequest
{
    [Key]
    public int ContactRequestId { get; set; }

    [StringLength(20)]
    public string TargetType { get; set; } = null!;

    public int TargetId { get; set; }

    public int? UserId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string Phone { get; set; } = null!;

    public string Message { get; set; } = null!;

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ContactRequests")]
    public virtual User? User { get; set; }
}
