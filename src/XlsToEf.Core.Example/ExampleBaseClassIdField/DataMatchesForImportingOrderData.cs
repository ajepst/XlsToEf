using MediatR;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.ExampleBaseClassIdField
{
    public class DataMatchesForImportingOrderData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}