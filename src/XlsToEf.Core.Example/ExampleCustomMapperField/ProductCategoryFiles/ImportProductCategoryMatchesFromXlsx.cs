using System.Threading;
using System.Threading.Tasks;
using MediatR;
using XlsToEfCore.Example.Domain;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleCustomMapperField.ProductCategoryFiles
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