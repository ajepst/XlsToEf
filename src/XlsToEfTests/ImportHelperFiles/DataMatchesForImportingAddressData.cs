using MediatR;
using XlsToEf.Import;

namespace XlsToEfTests.ImportHelperFiles
{
    public class DataMatchesForImportingAddressData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}