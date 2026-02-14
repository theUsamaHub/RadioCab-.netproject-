namespace RadioCab.Models
{
    public class FaqVM
    {
        public List<Faq> faqlist { get; set; } = new();
        public FaqValidate faqform { get; set; } = null!;
    }
}
