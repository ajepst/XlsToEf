using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public class ImportOrderMatchesFromXlsx : IAsyncRequestHandler<DataMatchesForImportingOrderData, ImportResult>
    {
        private readonly XlsxToTableImporter _xlsxToTableImporter;

        public ImportOrderMatchesFromXlsx(XlsxToTableImporter xlsxToTableImporter)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
        }

        public async Task<ImportResult> Handle(DataMatchesForImportingOrderData message)
        {
            return await _xlsxToTableImporter.ImportColumnData<Order>(message);
        }
    }
}