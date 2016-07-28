using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class ImportOrderMatchesFromXlsx : IAsyncRequestHandler<ImportMatchingProductCategoryData, ImportResult>
    {
        private readonly XlsxToTableImporter _xlsxToTableImporter;

        public ImportOrderMatchesFromXlsx(XlsxToTableImporter xlsxToTableImporter)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
        }

        public async Task<ImportResult> Handle(ImportMatchingProductCategoryData message)
        {
            return await _xlsxToTableImporter.ImportColumnData<ProductCategory>(message);
        }
    }
}