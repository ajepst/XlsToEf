using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class ImportMatchingProductCategoryData : ImportMatchingData, IAsyncRequest<ImportResult>
    {
    }
}