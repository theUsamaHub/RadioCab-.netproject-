namespace RadioCab.Models
{
    public class CityPageVM
    {
        public CityFormVM City_Validate { get; set; } = new();
        public List<CityListVM> City_list { get; set; } = new();
    }
}
