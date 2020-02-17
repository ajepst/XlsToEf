using MediatR;
using XlsToEf.Import;

namespace XlsToEfTests.ImportHelperFiles
{
    public class DataMatchesForImportingProductData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}