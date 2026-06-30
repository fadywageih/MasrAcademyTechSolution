using MasrAcademyTech.DAL.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrAcademyTech.DAL.Presistance.Data.Configurations
{
    public class CourseCodeConfiguration : IEntityTypeConfiguration<CourseCode>
    {
        public void Configure(EntityTypeBuilder<CourseCode> builder)
        {
            builder.Property(cc => cc.Code)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.HasOne(cc => cc.Course)
                   .WithMany()
                   .HasForeignKey(cc => cc.CourseId);
        }
    }
}
