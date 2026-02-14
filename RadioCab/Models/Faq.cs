using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

[Table("Faq")]
public partial class Faq
{
    [Key]
    public int FaqId { get; set; }

    [StringLength(300)]
    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;
}
