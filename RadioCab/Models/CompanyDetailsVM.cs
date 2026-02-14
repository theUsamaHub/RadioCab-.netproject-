namespace RadioCab.Models
{
    public class CompanyDetailsVM
    {
        // Company
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string ContactPerson { get; set; } = null!;
        public string? Designation { get; set; }
        public string? Address { get; set; }
        public string? Telephone { get; set; }
        public string? FaxNumber { get; set; }
        public string? CompanyEmail { get; set; }
        public string? Description { get; set; }
        public string? CompanyLogo { get; set; }
        public string? FBRCertificate { get; set; }
        public string? BusinessLicense { get; set; }
        public string RegisterationStatus { get; set; } = null!;

        // User
        public int UserID { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string? UserPhone { get; set; }
        public string UserStatus { get; set; } = null!;

        // City
        public int CityId { get; set; }
        public string CityName { get; set; } = null!;
        public string ZipCode { get; set; } = null!;

        // Membership
        public int MembershipId { get; set; }
        public string MembershipName { get; set; } = null!;
        public string? MembershipDescription { get; set; }
    }
}
