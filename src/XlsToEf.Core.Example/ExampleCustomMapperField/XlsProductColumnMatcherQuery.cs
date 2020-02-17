using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleCustomMapperField
{
    public class XlsProductColumnMatcherQuery : XlsxColumnMatcherQuery, IRequest<DataForMatcherUi>
    {
    }
}