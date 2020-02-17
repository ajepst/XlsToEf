using System.Threading;
using System.Threading.Tasks;
using MediatR;
using XlsToEfCore.Example.Domain;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleBaseClassIdField
{
    public class ImportOrderMatchesFromXlsx : IRequestHandler<DataMatchesForImportingOrderData, ImportResult>
    {
        private readonly XlsxToTableImporter _xlsxToTableImporter;

        public ImportOrderMatchesFromXlsx(XlsxToTableImporter xlsxToTableImporter)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
        }

        public Task<ImportResult> Handle(DataMatchesForImportingOrderData message, CancellationToken cancellationToken)
        {
            return _xlsxToTableImporter.ImportColumnData<Order>(message);
        }
    }
}