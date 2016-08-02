using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests.Infrastructure
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