namespace RadioCab.Models
{
    public class PaymentAmountVM
    {
        public PaymentAmountValidate paymentvalidate_form { get; set; } = null!;
        public List<PaymentAmount> paymentAmount_list { get; set; } = new();
        public List<Membership> MembershipList { get; set; } = new();
    }
}
