using MasrAcademyTech.DAL.Models.Courses;
using MasrAcademyTech.DAL.Presistance.UnitOfWork;
namespace MasrAcademyTech.BLL.Services.Activation
{
    public class ActivationService : IActivationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActivationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateCodeAsync(int courseId, string userId)
        {
            var code = new CourseCode
            {
                Code = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                CourseId = courseId,
                IsUsed = false,
                UsedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CourseCode>().AddAsync(code);
            await _unitOfWork.CompleteAsync();

            return code.Code;
        }

        public async Task<bool> ActivateCodeAsync(string code, string userId)
        {
            var codes = await _unitOfWork.Repository<CourseCode>()
                .FindAsync(c => c.Code == code && !c.IsUsed);

            var courseCode = codes.FirstOrDefault();
            if (courseCode == null) return false;

            courseCode.IsUsed = true;
            courseCode.UsedAt = DateTime.UtcNow;
            courseCode.UsedByUserId = userId;

            var userCourse = new UserCourse
            {
                UserId = userId,
                CourseId = courseCode.CourseId,
                EnrolledAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<UserCourse>().AddAsync(userCourse);
            _unitOfWork.Repository<CourseCode>().Update(courseCode);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
