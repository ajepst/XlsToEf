using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField
{
    public class BuildXlsxProductTableMatcher : IAsyncRequestHandler<XlsProductColumnMatcherQuery, DataForMatcherUi>
    {
        private readonly IExcelIoWrapper _excelIoWrapper;

        public BuildXlsxProductTableMatcher(IExcelIoWrapper excelIoWrapper)
        {
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<DataForMatcherUi> Handle(XlsProductColumnMatcherQuery message)
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