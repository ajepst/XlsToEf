using System.Threading.Tasks;
using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public class ImportOrderMatchesFromXlsx : IAsyncRequestHandler<ImportMatchingOrderData, ImportResult>
    {
        private readonly IdDefaultImporter _xlsxToTableImporter;

        public ImportOrderMatchesFromXlsx(IdDefaultImporter xlsxToTableImporter)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
        }

        public async Task<ImportResult> Handle(ImportMatchingOrderData message)
        {
            return await _xlsxToTableImporter.ImportColumnData<Domain.Order, short>(message);
        }
    }
}