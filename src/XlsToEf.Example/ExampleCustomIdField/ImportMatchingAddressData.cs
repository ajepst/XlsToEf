using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomIdField
{
    public abstract class ImportMatchingAddressData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}