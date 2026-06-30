using MasrAcademyTech.DAL.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrAcademyTech.DAL.Presistance.Data.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.Property(p => p.PaymentId)
                   .HasMaxLength(100);

            builder.Property(p => p.Status)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(p => p.Amount)
                   .HasColumnType("decimal(10,2)");
        }
    }
}
