using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class XlsxProductCategoryColumnMatcherQuery : XlsxColumnMatcherQuery, IAsyncRequest<DataForMatcherUi>
    {
    }
}