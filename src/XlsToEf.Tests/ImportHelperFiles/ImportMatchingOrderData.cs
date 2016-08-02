using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Tests.ImportHelperFiles
{
    public class ImportMatchingOrderData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}