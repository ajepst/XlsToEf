using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Tests.ImportHelperFiles
{
    public class ImportMatchingAddressData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}