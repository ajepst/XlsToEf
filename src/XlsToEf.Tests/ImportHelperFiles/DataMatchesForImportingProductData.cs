using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Tests.ImportHelperFiles
{
    public class DataMatchesForImportingProductData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}