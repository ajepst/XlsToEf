using MediatR;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class XlsxProductCategoryColumnMatcherQuery : XlsxColumnMatcherQuery, IRequest<DataForMatcherUi>
    {
    }
}