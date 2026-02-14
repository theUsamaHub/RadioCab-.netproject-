using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Index("Email", Name = "UQ__Users__A9D1053443CCF8B5", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserID { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string Password { get; set; } = null!;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(20)]
    public string? Role { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    [InverseProperty("User")]
    public virtual ICollection<ContactRequest> ContactRequests { get; set; } = new List<ContactRequest>();

    [InverseProperty("User")]
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    [InverseProperty("User")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
