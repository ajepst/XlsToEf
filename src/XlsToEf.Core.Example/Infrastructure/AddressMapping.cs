using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XlsToEfCore.Example.Domain;

namespace XlsToEfCore.Example.Infrastructure
{
    public class AddressMapping : IEntityTypeConfiguration<Address>
    {

        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");
            builder.HasKey(m => m.AddrId);
            builder.Property(m => m.AddrId).HasColumnName("AddrID").HasColumnType("nvarchar").HasMaxLength(50).IsRequired().ValueGeneratedNever();
            builder.Property(m => m.AddressLine1);
        }
    }
}