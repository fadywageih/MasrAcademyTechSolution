
namespace MasrAcademyTech.DAL.Models.Courses
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
