namespace RadioCab.Models
{
    public class Dashboard
    {

        public List<CompanyVacancy> CompanyVacancy_list { get; set; } = new List<CompanyVacancy>();
        public List<CompanyDetailsVM> Company_list { get; set; } = new();
        public List<Payment> Payment_list { get; set; } = new();

        public List<Company> company_list { get; set; } = new();
        public List<Driver> driver_list { get; set; } = new();

        public List<CompanyFeedback> Companyfeedback_list = new();
        public List<DriverFeedback> Driverfeedback_list = new();
        public List<City> cities { get; set; } = new();
        public List<Service> service_list { get; set; } = new();
        public List<DriverService> DriverService_list { get; set; } = new();
        public List<PlatformService> PlatformService_list { get; set; } = new();
        public List<DriverDetailsVM> Driver_list { get; set; } = new();
        public List<Membership> membership_list { get; set; } = new();
        public List<PaymentAmount> PaymentAmount_list { get; set; } = new();
        public List<PaymentMethod> PaymentMethod_list { get; set; } = new();
        public List<Feedback> feedback_list { get; set; } = new();
        public List<Feature> featurelist { get; set; } = new();
        public List<MembershipFeature> membershipfeaturelist { get; set; } = new();
        public List<User> User_list { get; set; } = new();

    }
}
