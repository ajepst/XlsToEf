using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToExcel;

namespace XlsToEf.Import
{
    public interface IExcelIoWrapper
    {
        Task<IList<string>> GetSheets(string filePath);
        Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName);
        Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery);
    }

    public class ExcelIoWrapper : IExcelIoWrapper
    {
        public async Task<IList<string>> GetSheets(string filePath)
        {
            var sheetNames = await Task.Run(() =>
            {
                using (var excel = new ExcelQueryFactory(filePath))
                {
                    return excel.GetWorksheetNames().ToList();
                }
            });

            return sheetNames;
        }

        private async Task<IList<string>> GetColumns(string filePath, string sheet)
        {
            var colNames = await Task.Run(() =>
            {
                using (var excel = new ExcelQueryFactory(filePath))
                {
                    return excel.GetColumnNames(sheet).ToList();
                }
            });

            return colNames;
        }

        public async Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery)
        {
            return await GetColumns(matcherQuery.FilePath, matcherQuery.Sheet);
        }

        public async Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName)
        {
            var worksheetRows = await Task.Run(() =>
            {
                using (var excel = new ExcelQueryFactory(filePath))
                {
                    return excel.Worksheet(sheetName)
                        .Select(row => row.ColumnNames.ToDictionary<string, string, string>(col => col, col => row[col]))
                        .ToList();
                }
            });
            return worksheetRows;
        }
    }
}