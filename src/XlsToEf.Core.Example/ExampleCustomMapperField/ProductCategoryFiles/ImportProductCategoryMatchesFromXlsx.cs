using System.Threading;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Core.Example.Domain;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class ImportProductCategoryMatchesFromXlsx : IRequestHandler<DataMatchesForImportingProductCategoryData, ImportResult>
    {
        private readonly XlsxToTableImporter _xlsxToTableImporter;

        public ImportProductCategoryMatchesFromXlsx(XlsxToTableImporter xlsxToTableImporter)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
        }

        public Task<ImportResult> Handle(DataMatchesForImportingProductCategoryData message, CancellationToken cancellationToken)
        {
            return _xlsxToTableImporter.ImportColumnData<ProductCategory>(message);
        }
    }
}