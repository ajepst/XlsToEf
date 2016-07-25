using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using XlsToEf.Example.Domain;

namespace XlsToEf.Example.Infrastructure
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