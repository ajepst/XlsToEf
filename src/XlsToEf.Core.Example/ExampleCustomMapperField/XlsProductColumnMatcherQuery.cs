using MediatR;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.ExampleCustomMapperField
{
    public class XlsProductColumnMatcherQuery : XlsxColumnMatcherQuery, IRequest<DataForMatcherUi>
    {
    }
}