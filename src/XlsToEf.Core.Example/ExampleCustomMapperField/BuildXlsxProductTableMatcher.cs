using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Core.Example.Domain;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.ExampleCustomMapperField
{
    public class BuildXlsxProductTableMatcher : IRequestHandler<XlsProductColumnMatcherQuery, DataForMatcherUi>
    {
        private readonly IExcelIoWrapper _excelIoWrapper;

        public BuildXlsxProductTableMatcher(IExcelIoWrapper excelIoWrapper)
        {
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<DataForMatcherUi> Handle(XlsProductColumnMatcherQuery message, CancellationToken cancellationToken)
        {
            message.FilePath = Path.GetTempPath() + message.FileName;
            var product = new Product();

            var columnData = new DataForMatcherUi
            {
                XlsxColumns = (await _excelIoWrapper.GetImportColumnData(message)).ToArray(),
                FileName = message.FileName,
                TableColumns = new List<TableColumnConfiguration>
                {
                    TableColumnConfiguration.Create("ProductCategoryCode", new SingleColumnData("Category Code")),
                    TableColumnConfiguration.Create(() => product.ProductName, new SingleColumnData("Product Name", required:false)),
                }
            };

            return columnData;
        }
    }
}