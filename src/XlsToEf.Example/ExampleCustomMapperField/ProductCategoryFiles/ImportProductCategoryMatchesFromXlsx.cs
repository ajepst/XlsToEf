using System.Threading;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class ImportProductCategoryMatchesFromXlsx : IRequestHandler<DataMatchesForImportingProductCategoryData, ImportResult>
    {
        private readonly XlsxToTableImporter _xlsxToTableImporter;

        public ImportProductCategoryMatchesFromXlsx(XlsxToTableImporter xlsxToTableImporter)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
        }

        public async Task<ImportResult> Handle(DataMatchesForImportingProductCategoryData message, CancellationToken cancellationToken)
        {
            return await _xlsxToTableImporter.ImportColumnData<ProductCategory>(message);
        }
    }
}