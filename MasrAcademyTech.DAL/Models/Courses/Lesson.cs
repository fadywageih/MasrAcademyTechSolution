namespace MasrAcademyTech.DAL.Models.Courses
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? VideoPath { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
