using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public class DataMatchesForImportingOrderData : DataMatchesForImport, IAsyncRequest<ImportResult>
    {
    }
}