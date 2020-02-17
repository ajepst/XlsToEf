using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class XlsxProductCategoryColumnMatcherQuery : XlsxColumnMatcherQuery, IRequest<DataForMatcherUi>
    {
    }
}