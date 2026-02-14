using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class CompanyServiceInput
    {
        [Required(ErrorMessage = "Please select a service")]
        public int SelectedServiceId { get; set; }
    }
}
