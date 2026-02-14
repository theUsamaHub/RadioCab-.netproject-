namespace RadioCab.Models
{
    public class ServiceVM
    {
        public List<Service> service_list { get; set; } = new();

        public ServiceValidate Service_Form { get; set; } = null!;
    }
}
