namespace RadioCab.Models.ViewModels
{
    public class CompanyVM
    {
        public Company Company { get; set; } = null!;
        public double AverageRating { get; set; }
        public int ServicesCount { get; set; }
        public int FeedbackCount { get; set; }
    }
}