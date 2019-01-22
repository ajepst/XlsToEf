using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XlsToEf.Core.Tests.Models;

namespace XlsToEf.Core.Tests.Infrastructure
{
    public class ProductMapping : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedOnAdd();
            builder.HasOne(x => x.ProductCategory).WithMany().HasForeignKey(x => x.ProductCategoryId);
            builder.Property(x => x.ProductCategoryId);
        }
    }
}