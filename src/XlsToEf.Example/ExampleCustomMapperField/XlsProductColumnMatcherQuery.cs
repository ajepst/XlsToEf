using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField
{
    public class XlsProductColumnMatcherQuery : XlsxColumnMatcherQuery, IRequest<DataForMatcherUi>
    {
    }
}