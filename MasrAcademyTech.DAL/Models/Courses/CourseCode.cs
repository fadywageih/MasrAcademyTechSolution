namespace MasrAcademyTech.DAL.Models.Courses
{
    public class CourseCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public bool IsUsed { get; set; }
        public string? UsedByUserId { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Course? Course { get; set; }
    }
}
