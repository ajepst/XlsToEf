using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class DataMatchesForImportingProductCategoryData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}