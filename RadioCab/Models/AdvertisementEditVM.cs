using System.ComponentModel.DataAnnotations;
namespace RadioCab.Models
{
    public class AdvertisementEditVM
    {
        public int AdvertisementId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public string ExistingImage { get; set; }
    }

}
