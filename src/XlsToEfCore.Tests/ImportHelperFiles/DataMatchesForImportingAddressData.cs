using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Tests.ImportHelperFiles
{
    public class DataMatchesForImportingAddressData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}