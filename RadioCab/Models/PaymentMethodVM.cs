namespace RadioCab.Models
{
    public class PaymentMethodVM
    {
        public List<PaymentMethod> paymentMethods_list { get; set; } = new();
        public PaymentMethodValidate paymentMethodValidateForm { get; set; } = null!;
    }
}
