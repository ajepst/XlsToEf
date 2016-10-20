using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class BuildXlsxProductCategoryTableMatcher : IAsyncRequestHandler<XlsxProductCategoryColumnMatcherQuery, DataForMatcherUi>
    {
        private readonly IExcelIoWrapper _excelIoWrapper;

        public BuildXlsxProductCategoryTableMatcher(IExcelIoWrapper excelIoWrapper)
        {
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<DataForMatcherUi> Handle(XlsxProductCategoryColumnMatcherQuery message)
        {
            message.FilePath = Path.GetTempPath() + message.FileName;
            var cat = new ProductCategory();

            var columnData = new DataForMatcherUi
            {
                XlsxColumns = (await _excelIoWrapper.GetImportColumnData(message)).ToArray(),
                FileName = message.FileName,
                TableColumns = new List<TableColumnConfiguration>
                {
                    TableColumnConfiguration.Create(() => cat.Id, new SingleColumnData("Category Id")),
                    TableColumnConfiguration.Create(() => cat.CategoryName, new SingleColumnData("Category Name")),
                    TableColumnConfiguration.Create(() => cat.CategoryCode, new SingleColumnData("Category Code")),
                }
            };

            return columnData;
        }
    }
}