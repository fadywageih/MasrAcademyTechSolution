namespace MasrAcademyTech.DAL.Models.Courses
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? CourseCodeId { get; set; }
        public string? PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
