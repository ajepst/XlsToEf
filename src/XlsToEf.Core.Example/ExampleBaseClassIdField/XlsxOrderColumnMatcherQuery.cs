using MediatR;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.ExampleBaseClassIdField
{
    public class XlsxOrderColumnMatcherQuery : XlsxColumnMatcherQuery, IRequest<DataForMatcherUi>
    {
    }
}