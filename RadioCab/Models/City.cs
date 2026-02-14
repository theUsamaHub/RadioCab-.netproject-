using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("City")]
[Index("ZipCode", Name = "UQ__City__2CC2CDB8172FEFF0", IsUnique = true)]
public partial class City
{
    [Key]
    public int CityId { get; set; }

    [StringLength(100)]
    public string CityName { get; set; } = null!;

    [StringLength(10)]
    public string ZipCode { get; set; } = null!;

    [InverseProperty("City")]
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    [InverseProperty("City")]
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    [InverseProperty("City")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
