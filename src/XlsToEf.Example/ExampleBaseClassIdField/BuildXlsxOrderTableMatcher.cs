using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{
    public class BuildXlsxOrderTableMatcher : ImportColumnDataBuilder, IAsyncRequestHandler<XlsxOrderColumnMatcherQuery, ImportColumnData>
    {
        public BuildXlsxOrderTableMatcher(IExcelIoWrapper excelIoWrapper)
            : base(excelIoWrapper)
        {
        }

        public async Task<ImportColumnData> Handle(XlsxOrderColumnMatcherQuery message)
        {
            message.FilePath = Path.GetTempPath() + message.FileName;
            var unit = new Order();

            var columnData = new ImportColumnData
            {
                XlsxColumns = (await GetImportColumnData(message)).ToArray(),
                FileName = message.FileName,
                TableColumns = new Dictionary<string, SingleColumnData>
                {
                    {GetPropertyName(() => unit.Id), new SingleColumnData("Unit ID")},
                    {GetPropertyName(() => unit.OrderDate), new SingleColumnData("Order Date")},
                }
            };

            return columnData;
        }
    }
}