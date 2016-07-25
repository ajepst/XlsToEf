using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests.Infrastructure
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