using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using XlsToEf.Example.Domain;

namespace XlsToEf.Example.Infrastructure
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