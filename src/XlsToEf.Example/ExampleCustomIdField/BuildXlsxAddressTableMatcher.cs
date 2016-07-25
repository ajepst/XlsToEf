using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomIdField
{
    public class BuildXlsxAddressTableMatcher : IAsyncRequestHandler<XlsAddressColumnMatcherQuery, ImportColumnData>
    {
        private readonly IExcelIoWrapper _excelIoWrapper;

        public BuildXlsxAddressTableMatcher(IExcelIoWrapper excelIoWrapper)
        {
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<ImportColumnData> Handle(XlsAddressColumnMatcherQuery message)
        {
            message.FilePath = Path.GetTempPath() + message.FileName;
            var wo = new Address();

            var columnData = new ImportColumnData
            {
                XlsxColumns = (await _excelIoWrapper.GetImportColumnData(message)).ToArray(),
                FileName = message.FileName,
                TableColumns = new Dictionary<string, SingleColumnData>
                {
                    {PropertyNameHelper.GetPropertyName(() => wo.AddrId), new SingleColumnData("Address Id")},
                    {PropertyNameHelper.GetPropertyName(() => wo.AddressLine1), new SingleColumnData("Address Line 1", required:false)},
                }
            };

            return columnData;
        }
    }
}