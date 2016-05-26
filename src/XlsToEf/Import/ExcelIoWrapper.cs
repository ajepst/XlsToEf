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
        Task<IList<string>> GetColumns(string filePath, string sheet);
        Task<Dictionary<string, string>> GetFirstTwoColsSheetSlice(string filePath, string sheetName);
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

        public async Task<IList<string>> GetColumns(string filePath, string sheet)
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

        public async Task<Dictionary<string, string>> GetFirstTwoColsSheetSlice(string filePath, string sheetName)
        {
            return await Task.Run(() =>
            {
                using (var excel = new ExcelQueryFactory(filePath))
                {
                    var slice = excel.WorksheetNoHeader(sheetName).Skip(1).Where(row => row[0] != null && row[1] != null);

                    var dict = slice.ToDictionary(row => row[0].Value.ToString(), row => row[1].Value.ToString());
                    return dict;
                }
            });
        }
    }
}