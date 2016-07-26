using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests
{
    public class TestPropertyOverriderDbTests : DbTestBase
    {
        public async Task ShouldPopulateDestinationFromSource()
        {
            var dbContext = GetDb();
            var originalCategory = new ProductCategory {CategoryCode = "abc"};
            var newCategory = new ProductCategory {CategoryCode = "def"};
            const string awesomeNewName = "Awesome New Name";

            var destination = new Product {ProductCategory = originalCategory};

            PersistToDatabase(originalCategory, newCategory);

            var matches = new Dictionary<string, string>
            {
                {"ProductCategory", "cat"},
                {"ProductName", "name"},

            };


            var excelRow = new Dictionary<string, string>
            {
                {"cat", newCategory.CategoryCode},
                {"name", awesomeNewName},

            };

            var overrider = new ProductPropertyOverrider<Product>(dbContext);
            await overrider.UpdateProperties(destination, matches, excelRow);
            destination.ProductCategory.CategoryCode.ShouldBe(newCategory.CategoryCode);
            destination.ProductName.ShouldBe(awesomeNewName);
        }


        private class ProductPropertyOverrider<T> : UpdatePropertyOverrider<T> where T : Product
        {
            private readonly DbContext _context;

            public ProductPropertyOverrider(DbContext context)
            {
                _context = context;
            }

            public override async Task UpdateProperties(T destination1, Dictionary<string, string> matches,
                Dictionary<string, string> excelRow)
            {
                {
                    var product = new Product();
                    var productCategoryPropertyName =
                        PropertyNameHelper.GetPropertyName(() => product.ProductCategory);
                    var productPropertyName = PropertyNameHelper.GetPropertyName(() => product.ProductName);

                    foreach (var destinationProperty in matches.Keys)
                    {
                        var xlsxColumnName = matches[destinationProperty];
                        var value = excelRow[xlsxColumnName];
                        if (destinationProperty == productCategoryPropertyName)
                        {
                            var newCategory =
                                await _context.Set<ProductCategory>().Where(x => x.CategoryCode == value).FirstAsync();
                            if (newCategory == null)
                                throw new RowParseException("Category Code does not match a category");
                            destination1.ProductCategory = newCategory;
                        }
                        else if (destinationProperty == productPropertyName)
                        {
                            destination1.ProductName = value;
                        }
                    }
                }
            }
        }
    }
}