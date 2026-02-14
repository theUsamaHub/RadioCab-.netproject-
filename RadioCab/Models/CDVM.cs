namespace RadioCab.Models
{
    public class CDVM
    {  
        public List<Advertisement> ad_list {  get; set; } = new();
        // Add these for filtering

        public int? SelectedMembershipId { get; set; }
        public int? SelectedCityId { get; set; }

        public string SelectedJobType { get; set; } = string.Empty;
        public string SelectedStatus { get; set; } = string.Empty;
        public List<string> JobTypes { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
        public List<Company> company_list { get; set; } = new();
        public List<Driver> driver_list { get; set; } = new();
        public Advertisement single_ad { get; set; } = null!;
        public List<CompanyVacancy> CompanyVacancy_list { get; set; } = new();
        public CompanyVacancy Single_vacancy { get; set; } = null!;
        public List<CompanyDetailsVM> Company_list { get; set; } = new();
        public CompanyDetailsVM Single_Company { get; set; } = null!;
        public List<Payment> Payment_list { get; set; } = new();

        public List<CompanyFeedback> Companyfeedback_list = new();
        public List<DriverFeedback> Driverfeedback_list = new();
        public List<City> cities { get; set; } = new();
        public List<Service> service_list { get; set; } = new();
        public Service Single_service { get; set; } = null!;


        public DriverService DriverService_Single { get; set; } = null!;

        public List<DriverService> DriverService_list { get; set; } = new();


        public CompanyService SingleCompanyService { get; set; } = null!;
        public List<CompanyService> CompanyService_list { get; set; } = new();


        public PlatformService PlatformService_Details { get; set; } = null!;
        public List<CityListVM> City_list { get; set; } = new();
        public List<DriverDetailsVM> Driver_list { get; set; } = new();

        public DriverDetailsVM Single_Driver { get; set; } = null!;

        public Payment Single_Payment { get; set; } = null!;

        public List<Membership> membership_list { get; set; } = new();

        public List<PaymentAmount> PaymentAmount_list { get; set; } = new();

        public List<PaymentMethod> PaymentMethod_list { get; set; } = new();
        public List<Feedback> feedback_list { get; set; } = new();

        public List<User> User_list { get; set; } = new();

        public User Single_User { get; set; } = null!;
    }
}
