using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XlsToEf.Core.Tests.Models;

namespace XlsToEf.Core.Tests.Infrastructure
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