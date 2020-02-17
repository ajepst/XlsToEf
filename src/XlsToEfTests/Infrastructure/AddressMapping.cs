using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using XlsToEfTests.Models;

namespace XlsToEfTests.Infrastructure
{
    public class AddressMapping : EntityTypeConfiguration<Address>
    {
        public AddressMapping()
        {
            ToTable("Addresses");
            HasKey(m => m.AddrId);
            Property(m => m.AddrId).HasColumnName("AddrID").HasColumnType("nvarchar").HasMaxLength(50).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(m => m.AddressLine1);
        }
    }
}