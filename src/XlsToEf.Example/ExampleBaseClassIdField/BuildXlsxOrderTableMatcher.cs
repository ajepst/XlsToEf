using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public class BuildXlsxOrderTableMatcher : IAsyncRequestHandler<XlsxOrderColumnMatcherQuery, ImportColumnData>
    {
        private readonly IExcelIoWrapper _excelIoWrapper;

        public BuildXlsxOrderTableMatcher(IExcelIoWrapper excelIoWrapper)
        {
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<ImportColumnData> Handle(XlsxOrderColumnMatcherQuery message)
        {
            message.FilePath = Path.GetTempPath() + message.FileName;
            var unit = new Order();

            var columnData = new ImportColumnData
            {
                XlsxColumns = (await _excelIoWrapper.GetImportColumnData(message)).ToArray(),
                FileName = message.FileName,
                TableColumns = new Dictionary<string, SingleColumnData>
                {
                    {PropertyNameHelper.GetPropertyName(() => unit.Id), new SingleColumnData("Unit ID")},
                    {PropertyNameHelper.GetPropertyName(() => unit.OrderDate), new SingleColumnData("Order Date")},
                }
            };

            return columnData;
        }
    }
}