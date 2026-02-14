namespace RadioCab.Models
{
    public class MembershipFeatureVM
    {
        public List<MembershipFeature> MembershipFeature_list { get; set; } = new();

        public MembershipFeatureValidate MembershipFeature_form { get; set; } = null!;

        public bool ShowForm { get; set; }

    }
}
