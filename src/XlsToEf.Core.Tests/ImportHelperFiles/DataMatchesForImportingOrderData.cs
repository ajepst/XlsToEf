using MediatR;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Tests.ImportHelperFiles
{
    public class DataMatchesForImportingOrderData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}