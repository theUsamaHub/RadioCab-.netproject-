using System.ComponentModel.DataAnnotations;

namespace RadioCab.Models
{
    public class CityListVM
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = null!;
        public string ZipCode { get; set; } = null!;

    }
}
