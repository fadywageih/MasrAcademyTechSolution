using MasrAcademyTech.BLL.Services.Courses;
using MasrAcademyTech.BLL.Services.Lessons;
using MasrAcademyTech.DAL.Models.Courses;
using MasrAcademyTech.DAL.Presistance.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasrAcademyTech.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;
        private readonly IWebHostEnvironment _environment;
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(
            ICourseService courseService,
            ILessonService lessonService,
            IWebHostEnvironment environment,
            IUnitOfWork unitOfWork)
        {
            _courseService = courseService;
            _lessonService = lessonService;
            _environment = environment;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllAsync();
            return View(courses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    var uploads = Path.Combine(_environment.WebRootPath, "images", "courses");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);
                    course.ImagePath = $"/images/courses/{fileName}";
                }

                await _courseService.CreateAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course, IFormFile? image)
        {
            if (id != course.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    var uploads = Path.Combine(_environment.WebRootPath, "images", "courses");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);
                    course.ImagePath = $"/images/courses/{fileName}";
                }

                await _courseService.UpdateAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _courseService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Lessons(int id)
        {
            ViewBag.CourseId = id;
            var lessons = await _lessonService.GetByCourseIdAsync(id);
            return View(lessons);
        }

        public IActionResult AddLesson(int courseId)
        {
            return View(new Lesson { CourseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(Lesson lesson, IFormFile? video)
        {
            if (ModelState.IsValid)
            {
                if (video != null)
                {
                    var uploads = Path.Combine(_environment.WebRootPath, "..", "Videos");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(video.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await video.CopyToAsync(stream);
                    lesson.VideoPath = fileName;
                }

                await _lessonService.CreateAsync(lesson);
                return RedirectToAction(nameof(Lessons), new { id = lesson.CourseId });
            }
            return View(lesson);
        }

        public async Task<IActionResult> Tickets()
        {
            var tickets = await _unitOfWork.Repository<SupportTicket>().GetAllAsync();
            return View(tickets);
        }
    }
}