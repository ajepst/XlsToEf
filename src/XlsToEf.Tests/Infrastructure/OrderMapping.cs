using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests.Infrastructure
{
    public class OrderMapping : EntityTypeConfiguration<Order>
    {
        public OrderMapping()
        {
            ToTable("Orders");
            HasKey(m => m.Id);
            Property(m => m.Id)
                .HasColumnName("ID")
                .HasColumnType("int")
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(m => m.OrderDate);
            Property(m => m.DeliveryDate);
        }
    }
}