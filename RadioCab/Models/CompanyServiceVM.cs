namespace RadioCab.Models
{
    public class CompanyServiceVM
    {
        // already-added services
        public List<CompanyService> CompanyServices { get; set; } = new();

        // dropdown source
        public List<Service> AvailableServices { get; set; } = new();

        // form input
        public CompanyServiceInput Input { get; set; } = new();
        public Service Service { get; set; } // navigation
    }
}
