using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Example.Infrastructure;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField
{
    public class ImportProductsFromXlsx : IAsyncRequestHandler<ImportMatchingProductData, ImportResult>
    {
        private readonly XlsxToTableImporter _xlsxToTableImporter;
        private readonly ProductPropertyOverrider<Product> _productOverrider;

        public ImportProductsFromXlsx(XlsxToTableImporter xlsxToTableImporter, ProductPropertyOverrider<Product> productOverrider)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
            _productOverrider = productOverrider;
        }

        public async Task<ImportResult> Handle(ImportMatchingProductData message)
        {
            Func<int, Expression<Func<Product, bool>>> finderExpression = selectorValue => prod => prod.Id == selectorValue;
            return await _xlsxToTableImporter.ImportColumnData(message, finderExpression, overridingMapper:_productOverrider, saveBehavior: new ImportSaveBehavior {RecordMode = RecordMode.CreateOnly});
        }

    }

    public class ProductPropertyOverrider<T> : UpdatePropertyOverrider<T> where T : Product
    {
        private readonly DbContext _context;

        public ProductPropertyOverrider(XlsToEfDbContext context)
        {
            _context = context;
        }

        public override async Task UpdateProperties(T destination1, Dictionary<string, string> matches, Dictionary<string, string> excelRow)
        {
            {
                var product = new Product();
                var productCategoryPropertyName = "ProductCategoryCode";
                var productPropertyName = PropertyNameHelper.GetPropertyName(() => product.ProductName);

                foreach (var destinationProperty in matches.Keys)
                {
                    var xlsxColumnName = matches[destinationProperty];
                    var value = excelRow[xlsxColumnName];
                    if (destinationProperty == productCategoryPropertyName)
                    {
                        var newCategory =
                            await _context.Set<ProductCategory>().Where(x => x.CategoryCode == value).FirstOrDefaultAsync();
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