namespace RadioCab.Models
{
    public class CompanyFeedbackandPayment
    {
        public List<CompanyFeedback> CompanyFeedback { get; set; } = new();
        public List<Payment> Payment { get; set; } = new();
    }
}
