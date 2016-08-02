using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles
{
    public class BuildXlsxProductCategoryTableMatcher : IAsyncRequestHandler<XlsxProductCategoryColumnMatcherQuery, ImportColumnData>
    {
        private readonly IExcelIoWrapper _excelIoWrapper;

        public BuildXlsxProductCategoryTableMatcher(IExcelIoWrapper excelIoWrapper)
        {
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<ImportColumnData> Handle(XlsxProductCategoryColumnMatcherQuery message)
        {
            message.FilePath = Path.GetTempPath() + message.FileName;
            var cat = new ProductCategory();

            var columnData = new ImportColumnData
            {
                XlsxColumns = (await _excelIoWrapper.GetImportColumnData(message)).ToArray(),
                FileName = message.FileName,
                TableColumns = new Dictionary<string, SingleColumnData>
                {
                    {PropertyNameHelper.GetPropertyName(() => cat.Id), new SingleColumnData("Category Id")},
                    {PropertyNameHelper.GetPropertyName(() => cat.CategoryName), new SingleColumnData("Category Name")},
                    {PropertyNameHelper.GetPropertyName(() => cat.CategoryCode), new SingleColumnData("Category Code")},
                }
            };

            return columnData;
        }
    }
}