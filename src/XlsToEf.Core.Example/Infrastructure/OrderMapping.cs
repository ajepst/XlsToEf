using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XlsToEf.Core.Example.Domain;

namespace XlsToEf.Core.Example.Infrastructure
{
    public class OrderMapping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("ID")
                .HasColumnType("int")
                .IsRequired()
                .ValueGeneratedNever();
            builder.Property(m => m.OrderDate);
            builder.Property(m => m.DeliveryDate);
        }
    }
}