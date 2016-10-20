using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField
{
    public class DataMatchesForImportingProductData : DataMatchesForImport, IAsyncRequest<ImportResult>
    {
    }
}