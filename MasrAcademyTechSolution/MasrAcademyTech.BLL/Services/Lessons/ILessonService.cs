using MasrAcademyTech.DAL.Models.Courses;

namespace MasrAcademyTech.BLL.Services.Lessons
{
    public interface ILessonService
    {
        Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId);
        Task<Lesson?> GetByIdAsync(int id);
        Task<Lesson> CreateAsync(Lesson lesson);
        Task UpdateAsync(Lesson lesson);
        Task DeleteAsync(int id);
    }
}
