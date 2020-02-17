using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Tests.ImportHelperFiles
{
    public class DataMatchesForImportingProductData : DataMatchesForImport, IRequest<ImportResult>
    {
    }
}