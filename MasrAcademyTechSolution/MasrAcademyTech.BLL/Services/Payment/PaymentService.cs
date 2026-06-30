using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace MasrAcademyTech.BLL.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaymentService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string?> CreatePaymentIntent(decimal amount, string orderId, string paymentMethod = "card")
        {
            var apiKey = _configuration["Paymob:ApiKey"];
            var iframeId = paymentMethod == "wallet"
                ? _configuration["Paymob:IframeIdWallet"]
                : _configuration["Paymob:IframeId"];
            var integrationId = paymentMethod == "wallet"
                ? _configuration["Paymob:IntegrationIdWallet"]
                : _configuration["Paymob:IntegrationId"];

            // Step 1: Auth
            var authResponse = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = apiKey });

            var authData = await authResponse.Content.ReadFromJsonAsync<PaymobAuthResponse>();
            if (authData?.Token == null) return null;

            // Step 2: Create Order
            var orderResponse = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/ecommerce/orders",
                new
                {
                    auth_token = authData.Token,
                    delivery_needed = false,
                    amount_cents = (int)(amount * 100),
                    currency = "EGP",
                    merchant_order_id = orderId
                });

            var orderData = await orderResponse.Content.ReadFromJsonAsync<PaymobOrderResponse>();
            if (orderData?.Id == null) return null;

            // Step 3: Payment Key
            var paymentKeyResponse = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys",
                new
                {
                    auth_token = authData.Token,
                    amount_cents = (int)(amount * 100),
                    expiration = 3600,
                    order_id = orderData.Id,
                    billing_data = new
                    {
                        first_name = "Test",
                        last_name = "User",
                        phone_number = "01000000000",
                        email = "test@test.com",
                        apartment = "NA",
                        floor = "NA",
                        street = "NA",
                        building = "NA",
                        city = "Cairo",
                        country = "EG",
                        state = "Cairo"
                    },
                    currency = "EGP",
                    integration_id = int.Parse(integrationId!),
                    lock_order_when_paid = false
                });

            var paymentKeyData = await paymentKeyResponse.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>();

            // Step 4: Return iframe URL
            return $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentKeyData?.Token}";
        }

        private class PaymobAuthResponse { public string? Token { get; set; } }
        private class PaymobOrderResponse { public int? Id { get; set; } }
        private class PaymobPaymentKeyResponse { public string? Token { get; set; } }
    }
}