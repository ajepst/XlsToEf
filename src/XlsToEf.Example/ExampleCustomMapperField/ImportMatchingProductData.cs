using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField
{
    public class ImportMatchingProductData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}