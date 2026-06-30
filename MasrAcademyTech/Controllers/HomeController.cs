// HomeController.cs
using MasrAcademyTech.BLL.Services.Activation;
using MasrAcademyTech.BLL.Services.Courses;
using MasrAcademyTech.BLL.Services.EmailSettings;
using MasrAcademyTech.BLL.Services.Lessons;
using MasrAcademyTech.BLL.Services.Payment;
using MasrAcademyTech.BLL.Services.PaymentCallback;
using MasrAcademyTech.DAL.Models.Courses;
using MasrAcademyTech.DAL.Models.Identity;
using MasrAcademyTech.DAL.Presistance.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MasrAcademyTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IPaymentService _paymentService;
        private readonly IActivationService _activationService;
        private readonly ILessonService _lessonService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly IPaymentCallbackService _paymentCallbackService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSettings _emailSettings;

        public HomeController(
            ICourseService courseService,
            IPaymentService paymentService,
            IActivationService activationService,
            ILessonService lessonService,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment,
            IPaymentCallbackService paymentCallbackService,
            UserManager<ApplicationUser> userManager,
            IEmailSettings emailSettings)
        {
            _courseService = courseService;
            _paymentService = paymentService;
            _activationService = activationService;
            _lessonService = lessonService;
            _unitOfWork = unitOfWork;
            _environment = environment;
            _paymentCallbackService = paymentCallbackService;
            _userManager = userManager;
            _emailSettings = emailSettings;
        }

        public async Task<IActionResult> Index() => View();

        public async Task<IActionResult> Courses()
        {
            var courses = await _courseService.GetAllAsync();
            return View(courses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [Authorize]
        public async Task<IActionResult> Checkout(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null) return NotFound();
            HttpContext.Session.SetString("CourseId", id.ToString());
            return View(course);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int courseId, string paymentMethod = "card")
        {
            var course = await _courseService.GetByIdAsync(courseId);
            if (course == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var orderId = $"{userId}-{courseId}-{Guid.NewGuid().ToString("N")[..6]}";

            var paymentUrl = await _paymentService.CreatePaymentIntent(course.Price, orderId, paymentMethod);
            if (paymentUrl == null)
            {
                TempData["Error"] = "حدث خطأ أثناء الاتصال بخدمة الدفع";
                return RedirectToAction("Checkout", new { id = courseId });
            }

            return Redirect(paymentUrl);
        }
        [Authorize]
        public async Task<IActionResult> PaymentSuccess()
        {
            var courseId = HttpContext.Session.GetString("CourseId");
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(courseId) && !string.IsNullOrEmpty(userId))
            {
                var code = new CourseCode
                {
                    Code = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    CourseId = int.Parse(courseId),
                    UsedByUserId = userId,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<CourseCode>().AddAsync(code);

                var course = await _courseService.GetByIdAsync(int.Parse(courseId));
                if (course != null)
                {
                    ViewBag.Code = code.Code;
                    ViewBag.CourseTitle = course.Title;

                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
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

                await _unitOfWork.CompleteAsync();
                HttpContext.Session.Remove("CourseId");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PaymentCallback()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var success = await _paymentCallbackService.ProcessCallbackAsync(body);
            return success ? Ok() : BadRequest();
        }

        [Authorize]
        public IActionResult Activate() => View();

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "من فضلك أدخل الكود");
                return View();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var result = await _activationService.ActivateCodeAsync(code.Trim().ToUpper(), userId);
            if (!result)
            {
                ModelState.AddModelError("", "الكود غير صحيح أو مستخدم من قبل");
                return View();
            }

            TempData["Success"] = "تم تفعيل الكورس بنجاح!";
            return RedirectToAction("MyCourses");
        }

        [Authorize]
        public async Task<IActionResult> MyCourses()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var userCourses = await _unitOfWork.Repository<UserCourse>()
                .FindAsync(uc => uc.UserId == userId);

            var courses = new List<Course>();
            foreach (var uc in userCourses)
            {
                var course = await _courseService.GetByIdAsync(uc.CourseId);
                if (course != null) courses.Add(course);
            }

            return View(courses);
        }

        [Authorize]
        public async Task<IActionResult> Watch(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var hasAccess = await _unitOfWork.Repository<UserCourse>()
                .FindAsync(uc => uc.UserId == userId && uc.CourseId == id);

            if (!hasAccess.Any()) return RedirectToAction("MyCourses");

            var course = await _courseService.GetByIdAsync(id);
            if (course == null) return NotFound();

            var lessons = await _lessonService.GetByCourseIdAsync(id);
            ViewBag.Lessons = lessons;
            return View(course);
        }

        [Authorize]
        public async Task<IActionResult> StreamVideo(int lessonId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var lesson = await _lessonService.GetByIdAsync(lessonId);
            if (lesson == null) return NotFound();

            var hasAccess = await _unitOfWork.Repository<UserCourse>()
                .FindAsync(uc => uc.UserId == userId && uc.CourseId == lesson.CourseId);

            if (!hasAccess.Any()) return Unauthorized();

            var videoPath = Path.Combine(_environment.WebRootPath, "..", "Videos", lesson.VideoPath!);
            if (!System.IO.File.Exists(videoPath)) return NotFound();

            var stream = new FileStream(videoPath, FileMode.Open, FileAccess.Read);
            return File(stream, "video/mp4", enableRangeProcessing: true);
        }
        public IActionResult Support()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Support(SupportTicket ticket)
        {
            if (!ModelState.IsValid) return View(ticket);

            ticket.CreatedAt = DateTime.UtcNow;
            ticket.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Guest";

            await _unitOfWork.Repository<SupportTicket>().AddAsync(ticket);
            await _unitOfWork.CompleteAsync();

            TempData["Success"] = "تم إرسال تذكرتك بنجاح. سنتواصل معك قريباً.";
            return RedirectToAction("Support");
        }
    }
}