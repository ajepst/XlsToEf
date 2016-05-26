using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public abstract class ImportMatchingOrderData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}