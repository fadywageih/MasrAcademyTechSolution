namespace MasrAcademyTech.BLL.Services.PaymentCallback
{
    public interface IPaymentCallbackService
    {
        Task<bool> ProcessCallbackAsync(string jsonBody);
    }
}
