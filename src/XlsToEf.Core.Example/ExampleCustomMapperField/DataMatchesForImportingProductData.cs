using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleCustomMapperField
{
    public class DataMatchesForImportingProductData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}