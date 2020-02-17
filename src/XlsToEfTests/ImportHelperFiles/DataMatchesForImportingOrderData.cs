using MediatR;
using XlsToEf.Import;

namespace XlsToEfTests.ImportHelperFiles
{
    public class DataMatchesForImportingOrderData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}