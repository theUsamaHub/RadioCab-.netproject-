namespace RadioCab.Models
{
    public class FeatureVM
    {
        public List<Feature> Feature_list { get; set; } = new();
        public FeatureValidate FeatureForm { get; set; } = null!;
        public bool ShowForm { get; set; }

    }
}
