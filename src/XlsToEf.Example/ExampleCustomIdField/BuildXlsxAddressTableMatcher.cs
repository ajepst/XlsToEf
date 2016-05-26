using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomIdField
{
    public class BuildXlsxAddressTableMatcher : ImportColumnDataBuilder, IAsyncRequestHandler<XlsAddressColumnMatcherQuery, ImportColumnData>
    {
        public BuildXlsxAddressTableMatcher(IExcelIoWrapper excelIoWrapper)
            : base(excelIoWrapper)
        {
        }

        public async Task<ImportColumnData> Handle(XlsAddressColumnMatcherQuery message)
        {
            message.FilePath = Path.GetTempPath() + message.FileName;
            var wo = new Domain.Address();

            var columnData = new ImportColumnData
            {
                XlsxColumns = (await GetImportColumnData(message)).ToArray(),
                FileName = message.FileName,
                TableColumns = new Dictionary<string, SingleColumnData>
                {
                    {GetPropertyName(() => wo.AddrId), new SingleColumnData("Address Id")},
                    {GetPropertyName(() => wo.AddressLine1), new SingleColumnData("Address Line 1", required:false)},
                }
            };

            return columnData;
        }
    }
}