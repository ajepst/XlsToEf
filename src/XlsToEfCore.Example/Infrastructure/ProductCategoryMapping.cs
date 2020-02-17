using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XlsToEfCore.Example.Domain;

namespace XlsToEfCore.Example.Infrastructure
{
    public class ProductCategoryMapping : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.CategoryCode);
            builder.Property(x => x.CategoryName);
        }
    }
}