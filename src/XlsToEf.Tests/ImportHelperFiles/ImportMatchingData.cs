using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Tests.ImportHelperFiles
{
    public class ImportMatchingProductData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}