namespace RadioCab.Models
{
    public class MembershipVM
    {
        public MembershipValidate Membership_form { get; set; } = null!;
        public List<Membership> Membership_list { get; set; } = new();
    }
}
