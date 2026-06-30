namespace MasrAcademyTech.DAL.Models.Courses
{
    public class UserCourse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public Course? Course { get; set; }
    }
}
