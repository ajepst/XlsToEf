using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public class ImportMatchingOrderData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}