using MasrAcademyTech.DAL.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrAcademyTech.DAL.Presistance.Data.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.Property(c => c.Title)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(c => c.Description)
                   .HasMaxLength(500);

            builder.Property(c => c.Price)
                   .HasColumnType("decimal(10,2)");

            builder.HasMany(c => c.Lessons)
                   .WithOne(l => l.Course)
                   .HasForeignKey(l => l.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
