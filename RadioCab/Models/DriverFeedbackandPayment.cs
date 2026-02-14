namespace RadioCab.Models
{
    public class DriverFeedbackandPayment
    {
        public List<DriverFeedback> DriverFeedback { get; set; } = new();
        public List<Payment> Payment { get; set; } = new();
    }
}
