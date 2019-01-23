using MediatR;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.ExampleCustomMapperField
{
    public class DataMatchesForImportingProductData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}