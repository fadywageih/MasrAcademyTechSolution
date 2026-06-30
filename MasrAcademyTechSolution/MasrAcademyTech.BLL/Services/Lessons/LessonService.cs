using MasrAcademyTech.DAL.Models.Courses;
using MasrAcademyTech.DAL.Presistance.UnitOfWork;

namespace MasrAcademyTech.BLL.Services.Lessons
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LessonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId)
        {
            var lessons = await _unitOfWork.Repository<Lesson>()
                .FindAsync(l => l.CourseId == courseId);
            return lessons.OrderBy(l => l.Order);
        }

        public async Task<Lesson?> GetByIdAsync(int id)
            => await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);

        public async Task<Lesson> CreateAsync(Lesson lesson)
        {
            await _unitOfWork.Repository<Lesson>().AddAsync(lesson);
            await _unitOfWork.CompleteAsync();
            return lesson;
        }

        public async Task UpdateAsync(Lesson lesson)
        {
            _unitOfWork.Repository<Lesson>().Update(lesson);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
            if (lesson != null)
            {
                _unitOfWork.Repository<Lesson>().Delete(lesson);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
