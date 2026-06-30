using MasrAcademyTech.DAL.Models.Courses;
using MasrAcademyTech.DAL.Presistance.Data;
using MasrAcademyTech.DAL.Presistance.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace MasrAcademyTech.BLL.Services.Courses
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDBContext _context;

        public CourseService(IUnitOfWork unitOfWork, ApplicationDBContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
            => await _context.Courses.Include(c => c.Lessons).ToListAsync();

        public async Task<Course?> GetByIdAsync(int id)
            => await _context.Courses.Include(c => c.Lessons).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Course> CreateAsync(Course course)
        {
            await _unitOfWork.Repository<Course>().AddAsync(course);
            await _unitOfWork.CompleteAsync();
            return course;
        }

        public async Task UpdateAsync(Course course)
        {
            _unitOfWork.Repository<Course>().Update(course);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _unitOfWork.Repository<Course>().Delete(course);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}