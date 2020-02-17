using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Tests.ImportHelperFiles
{
    public class DataMatchesForImportingOrderData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}