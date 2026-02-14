using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("PaymentMethod")]
[Index("MethodName", Name = "UQ__PaymentM__218CFB1759D59EC7", IsUnique = true)]
public partial class PaymentMethod
{
    [Key]
    public int PaymentMethodId { get; set; }

    [StringLength(50)]
    public string MethodName { get; set; } = null!;

    public bool? IsActive { get; set; }

    [InverseProperty("PaymentMethod")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
