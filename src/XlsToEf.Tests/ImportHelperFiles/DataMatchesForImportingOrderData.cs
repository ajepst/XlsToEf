using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Tests.ImportHelperFiles
{
    public class DataMatchesForImportingOrderData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}