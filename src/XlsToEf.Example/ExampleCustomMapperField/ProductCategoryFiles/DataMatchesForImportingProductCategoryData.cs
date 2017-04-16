using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class DataMatchesForImportingProductCategoryData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}