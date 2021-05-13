using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace XlsToEfCore.Import
{
    public interface IExcelIoWrapper
    {
        Task<IList<string>> GetSheets(string filePath, FileFormat fileFormat);
        Task<IList<string>> GetSheets(Stream fileStream, FileFormat fileFormat);
        Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName, FileFormat fileFormat);
        Task<List<Dictionary<string, string>>> GetRows(Stream fileStream, string sheetName, FileFormat fileFormat);
        Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery);
    }

    public class ExcelIoWrapper : IExcelIoWrapper
    {
        public async Task<IList<string>> GetSheets(string filePath, FileFormat fileFormat)
        {
            using (var stream = new FileInfo(filePath).OpenRead())
            {
                return await GetSheets(stream, fileFormat);
            }
        }

        public async Task<IList<string>> GetSheets(Stream fileStream, FileFormat fileFormat)
        {
            var sheetNames = await Task.Run(() =>
            {
                using (var excel = new ExcelPackage(fileStream))
                {
                    return excel.Workbook.Worksheets.Select(x => x.Name).ToList();
                }
            });

            return sheetNames;

        }

        private async Task<IList<string>> GetColumns(string filePath, string sheetName)
        {
            using (var stream = new FileInfo(filePath).OpenRead())
            {
                return await GetColumns(stream, sheetName);
            }
        }

        private async Task<IList<string>> GetColumns(Stream fileStream, string sheetName)
        {
            var colNames = await Task.Run(() =>
            {
                using (var excel = new ExcelPackage(fileStream))
                {
                    EnsureSheetExists(sheetName, excel);

                    var sheet = excel.Workbook.Worksheets.First(x => x.Name == sheetName);
                    var headerCells =
                        sheet.Cells[
                            sheet.Dimension.Start.Row, sheet.Dimension.Start.Column, 1, sheet.Dimension.End.Column];
                    return headerCells.Select(x => x.Text).ToList();
                }
            });

            return colNames;
        }

        private static void EnsureSheetExists(string sheetName, ExcelPackage excel)
        {
            if (excel.Workbook.Worksheets.All(x => x.Name != sheetName))
                throw new SheetNotFoundException(sheetName);
        }

        public Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery)
        {
            if(matcherQuery.FileStream == null)
                return GetColumns(matcherQuery.FilePath, matcherQuery.Sheet);

            return GetColumns(matcherQuery.FileStream, matcherQuery.Sheet);
        }

        public async Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName,
            FileFormat fileFormat)
        {
            using (var stream = new FileInfo(filePath).OpenRead())
            {
                return await GetRows(stream, sheetName, FileFormat.OpenExcel);
            }
        }

        public async Task<List<Dictionary<string, string>>> GetRows(Stream fileStream, string sheetName, FileFormat fileFormat)
        {
            var worksheetRows = await Task.Run(() =>
            {
                using (var excel = new ExcelPackage(fileStream))
                {
                    EnsureSheetExists(sheetName, excel);

                    var sheet = excel.Workbook.Worksheets.First(x => x.Name == sheetName);

                    var rows = new List<Dictionary<string, string>>();

                    for (var rowNum = 2; rowNum <= sheet.Dimension.End.Row; rowNum++)
                    {
                        var rowDict = new Dictionary<string, string>();
                        var row = sheet.Cells[string.Format("{0}:{0}", rowNum)];

                        for (int colIndex = sheet.Dimension.Start.Column; colIndex <= sheet.Dimension.End.Column; colIndex++)
                        { // ... Cell by cell...
                            string cellValue = sheet.Cells[rowNum, colIndex].Text; // This got me the actual value I needed.
                            rowDict.Add(sheet.Cells[1, colIndex].Text, cellValue);
                        }
                        rows.Add(rowDict);
                    }

                    return rows;
                }
            });
            return worksheetRows;
        }
    }
}