using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.ExampleBaseClassIdField
{
    public class DataMatchesForImportingOrderData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}