using MasrAcademyTech.DAL.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrAcademyTech.DAL.Presistance.Data.Configurations
{
    public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
    {
        public void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            builder.Property(s => s.Subject)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(s => s.Message)
                   .HasMaxLength(2000)
                   .IsRequired();
        }
    }
}
