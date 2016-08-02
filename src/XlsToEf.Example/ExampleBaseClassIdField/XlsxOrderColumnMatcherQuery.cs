using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public class XlsxOrderColumnMatcherQuery : XlsxColumnMatcherQuery, IAsyncRequest<ImportColumnData>
    {
    }
}