// PaymentCallbackService.cs
using System.Text.Json;
using MasrAcademyTech.BLL.Services.EmailSettings;
using MasrAcademyTech.DAL.Models.Courses;
using MasrAcademyTech.DAL.Models.Identity;
using MasrAcademyTech.DAL.Presistance.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MasrAcademyTech.BLL.Services.PaymentCallback
{
    public class PaymentCallbackService : IPaymentCallbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSettings _emailSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PaymentCallbackService> _logger;

        public PaymentCallbackService(
            IUnitOfWork unitOfWork,
            IEmailSettings emailSettings,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentCallbackService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailSettings = emailSettings;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> ProcessCallbackAsync(string jsonBody)
        {
            try
            {
                var callback = JsonSerializer.Deserialize<PaymobWebhook>(jsonBody);
                if (callback?.Success != true) return false;

                var transaction = new PaymentTransaction
                {
                    PaymentId = callback.Order?.Id?.ToString() ?? Guid.NewGuid().ToString(),
                    Amount = (callback.AmountCents ?? 0) / 100m,
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);

                if (!string.IsNullOrEmpty(callback.MerchantOrderId))
                {
                    var parts = callback.MerchantOrderId.Split('-');
                    if (parts.Length >= 2)
                    {
                        var userId = parts[0];
                        var courseId = int.Parse(parts[1]);

                        var code = new CourseCode
                        {
                            Code = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                            CourseId = courseId,
                            UsedByUserId = userId,
                            IsUsed = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Repository<CourseCode>().AddAsync(code);

                        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(courseId);
                        var user = await _userManager.FindByIdAsync(userId);

                        if (course != null && user != null)
                        {
                            var email = new Email
                            {
                                To = user.Email!,
                                Subject = $"كود تفعيل كورس {course.Title} - MasrAcademyTech",
                                Body = $@"
                                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: auto; padding: 20px; border-radius: 12px; background-color: #f9fafb; text-align: center;'>
                                        <h2 style='color: #1e40af;'>🎉 تم الدفع بنجاح!</h2>
                                        <p style='color: #4b5563;'>شكراً {user.FullName} لشرائك كورس <strong>{course.Title}</strong></p>
                                        <div style='background: #2563eb; color: white; font-size: 28px; font-weight: bold; padding: 16px; border-radius: 8px; margin: 20px 0; letter-spacing: 6px;'>
                                            {code.Code}
                                        </div>
                                        <p style='color: #6b7280; font-size: 14px;'>استخدم هذا الكود لتفعيل الكورس في حسابك</p>
                                    </div>"
                            };
                            _emailSettings.SendEmail(email);
                        }
                    }
                }

                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback");
                return false;
            }
        }

        private class PaymobWebhook
        {
            public bool? Success { get; set; }
            public int? AmountCents { get; set; }
            public string? MerchantOrderId { get; set; }
            public PaymobOrderData? Order { get; set; }
        }

        private class PaymobOrderData
        {
            public int? Id { get; set; }
        }
    }
}