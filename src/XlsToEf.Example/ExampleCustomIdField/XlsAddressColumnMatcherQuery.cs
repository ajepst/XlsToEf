using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomIdField
{
    public class XlsAddressColumnMatcherQuery : XlsxColumnMatcherQuery, IAsyncRequest<ImportColumnData>
    {
    }
}