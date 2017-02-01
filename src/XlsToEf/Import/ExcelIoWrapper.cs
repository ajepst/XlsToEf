using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace XlsToEf.Import
{
    public interface IExcelIoWrapper
    {
        Task<IList<string>> GetSheets(string filePath);
        Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName, char? delimiter = null);
        Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery);
    }

    public class ExcelIoWrapper : IExcelIoWrapper
    {
        private const string DefaultSheetName = "Sheet1";

        public async Task<IList<string>> GetSheets(string filePath)
        {
            var sheetNames = await Task.Run(() =>
            {
                using (var excel = new ExcelPackage(new FileInfo(filePath)))
                {
                    if (IsCsvFile(filePath))
                    {
                        return new List<string> {DefaultSheetName}; 
                    }
  
                    return excel.Workbook.Worksheets.Select(x => x.Name).ToList();
                }
            });

            return sheetNames;
        }

        private async Task<IList<string>> GetColumns(string filePath, string sheetName, char? delimter = null)
        {
            var colNames = await Task.Run(() =>
            {
                using (var excel = new ExcelPackage())
                {
                    if (IsCsvFile(filePath) || delimter != null)
                    {
                        PopulateSheetFromCsv(filePath, excel, delimter);
                        sheetName = DefaultSheetName;
                    }
                    else
                    {
                        excel.Load(File.OpenRead(filePath));
                    }


                    var sheet = excel.Workbook.Worksheets.First(x => x.Name == sheetName);
                    var headerCells =
                        sheet.Cells[
                            sheet.Dimension.Start.Row, sheet.Dimension.Start.Column, 1, sheet.Dimension.End.Column];
                    return headerCells.Select(x => x.Text).ToList();
                }
            });

            return colNames;
        }

        protected static void PopulateSheetFromCsv(string filePath, ExcelPackage excel, char? delimter)
        {
            var sheet1 = excel.Workbook.Worksheets.Add(DefaultSheetName);
            var excelTextFormat = new ExcelTextFormat {Delimiter = delimter ?? ',', };
            sheet1.Cells["A1"].LoadFromText(new FileInfo(filePath), excelTextFormat, TableStyles.Medium27, true);
        }

        public async Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery)
        {
            return await GetColumns(matcherQuery.FilePath, matcherQuery.Sheet, matcherQuery.Delimiter);
        }

        public async Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName, char? delimiter = null)
        {
            var worksheetRows = await Task.Run(() =>
            {
                using (var excel = new ExcelPackage())
                {
                    if (IsCsvFile(filePath)|| delimiter != null)
                    {
                        PopulateSheetFromCsv(filePath, excel, delimiter);
                        sheetName = DefaultSheetName;
                    }
                    else
                    {
                        excel.Load(File.OpenRead(filePath));
                    }
                
                    var sheet = excel.Workbook.Worksheets.First(x => x.Name == sheetName);


                    var rows = new List<Dictionary<string, string>>();

                    for (var rowNum = 2; rowNum <= sheet.Dimension.End.Row; rowNum++)
                    {
                        var rowDict = new Dictionary<string, string>();
                        var row = sheet.Cells[string.Format("{0}:{0}", rowNum)];

                        var rowCells = row.ToList();
                        for (var colIndex = 0; colIndex < sheet.Dimension.Columns; colIndex++)
                        {
                            var cell = rowCells[colIndex];
                            rowDict.Add(sheet.Cells[1, colIndex + 1].Text, cell.Text);
                        }
                        rows.Add(rowDict);
                    }

                    return rows;
                }
            });
            return worksheetRows;
        }

        protected bool IsCsvFile(string filePath )
        {
            var extension = Path.GetExtension(filePath);
            return extension != ".xlsx";
        }
    }
}