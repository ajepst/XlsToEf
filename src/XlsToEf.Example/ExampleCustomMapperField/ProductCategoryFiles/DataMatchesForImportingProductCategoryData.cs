using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class DataMatchesForImportingProductCategoryData : DataMatchesForImport, IAsyncRequest<ImportResult>
    {
    }
}