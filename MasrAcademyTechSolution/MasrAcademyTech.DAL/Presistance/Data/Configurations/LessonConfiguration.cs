using MasrAcademyTech.DAL.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrAcademyTech.DAL.Presistance.Data.Configurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.Property(l => l.Title)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(l => l.VideoPath)
                   .HasMaxLength(500);
        }
    }
}
