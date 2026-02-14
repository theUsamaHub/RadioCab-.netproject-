namespace RadioCab.Models
{
    public class DriverDetailsVM
    {

        public int DriverId { get; set; }
        public string DriverName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Telephone { get; set; }
        public string? DriverEmail { get; set; }
        public string? Experience { get; set; }
        public string? Description { get; set; }
        public string? DriverPhoto { get; set; }
        public string? DrivingLicenseNumber { get; set; }
        public string? DrivingLicenseFile { get; set; }
        public string? VehicleInfo { get; set; }
        public string RegisterationStatus { get; set; } = null!;

        // User
        public int UserId { get; set; }
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
