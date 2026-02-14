using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public int PaymentAmountId { get; set; }

    public int PaymentMethodId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string PaymentStatus { get; set; } = null!;

    [StringLength(100)]
    public string? TransactionId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PaymentDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpiryDate { get; set; }

    [StringLength(250)]
    public string? PaymentScreenshot { get; set; }

    [StringLength(30)]
    public string? PaymentPurpose { get; set; }

    [InverseProperty("Payment")]
    public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();

    [ForeignKey("PaymentAmountId")]
    [InverseProperty("Payments")]
    public virtual PaymentAmount PaymentAmount { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("Payments")]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Payments")]
    public virtual User User { get; set; } = null!;
}
