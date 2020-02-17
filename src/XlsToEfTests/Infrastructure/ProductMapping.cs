using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using XlsToEfTests.Models;

namespace XlsToEfTests.Infrastructure
{
    public class ProductMapping : EntityTypeConfiguration<Product>
    {
        public ProductMapping()
        {
            ToTable("Products");
            HasKey(m => m.Id);
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasRequired(x => x.ProductCategory).WithMany().HasForeignKey(x => x.ProductCategoryId);
            Property(x => x.ProductCategoryId);
        }
    }
}