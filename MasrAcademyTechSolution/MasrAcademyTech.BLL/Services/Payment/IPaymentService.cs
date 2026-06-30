namespace MasrAcademyTech.BLL.Services.Payment
{
    public interface IPaymentService
    {
        Task<string?> CreatePaymentIntent(decimal amount, string orderId, string paymentMethod = "card");
    }
}
