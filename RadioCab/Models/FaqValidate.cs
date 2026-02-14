using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class FaqValidate
    {
        public int FaqId { get; set; }

        [StringLength(300)]
        [Required(ErrorMessage = "question Name is required")]
        public string Question { get; set; } = null!;

        [Required(ErrorMessage = "Answer Name is required")]
        public string Answer { get; set; } = null!;
    }
}
