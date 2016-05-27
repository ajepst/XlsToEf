using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomIdField
{
    public class ImportMatchingAddressData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}