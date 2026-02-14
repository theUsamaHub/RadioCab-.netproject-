using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("PaymentAmount")]
[Index("MembershipId", "EntityType", "PaymentType", Name = "UQ_PaymentAmount", IsUnique = true)]
public partial class PaymentAmount
{
    [Key]
    public int PaymentAmountId { get; set; }

    public int MembershipId { get; set; }

    [StringLength(30)]
    public string EntityType { get; set; } = null!;

    [StringLength(30)]
    public string PaymentType { get; set; } = null!;

    public int DurationInMonths { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("MembershipId")]
    [InverseProperty("PaymentAmounts")]
    public virtual Membership Membership { get; set; } = null!;

    [InverseProperty("PaymentAmount")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
