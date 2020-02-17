using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleBaseClassIdField
{
    public class XlsxOrderColumnMatcherQuery : XlsxColumnMatcherQuery, IRequest<DataForMatcherUi>
    {
    }
}