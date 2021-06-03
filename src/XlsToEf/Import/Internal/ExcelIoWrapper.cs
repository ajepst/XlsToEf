using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;

namespace XlsToEf.Import
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

        public Task<IList<string>> GetSheets(Stream fileStream, FileFormat fileFormat)
        {
            var reader = GetReader(fileStream, fileFormat);
            var worksheetNames = GetAllSheetNames(reader);
            return Task.FromResult<IList<string>>(worksheetNames);
        }

        public async Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName, FileFormat fileFormat)
        {
            using (var stream = new FileInfo(filePath).OpenRead())
            {
                return await GetRows(stream, sheetName, fileFormat);
            }
        }

        public Task<List<Dictionary<string, string>>> GetRows(Stream fileStream, string sheetName,
            FileFormat fileFormat)
        {
            var reader = GetReader(fileStream, fileFormat);

            if (fileFormat == FileFormat.OpenExcel)
            {
                EnsureSheetExists(sheetName, reader);
                SelectSheetOnReader(sheetName, reader);
            }

            var columns = GetColumnsFromPreSelectedSheet(reader);
            var rows = new List<Dictionary<string, string>>();

            while (reader.Read())
            {
                var row = new Dictionary<string, string>();
                var record = (IDataRecord)reader;
                for (var field = 0; field < record.FieldCount; field++)
                {
                    row.Add(columns[field], record[field].ToString());
                }

                rows.Add(row);
            }

            return Task.FromResult(rows);
        }

        private static IExcelDataReader GetReader(Stream fileStream, FileFormat matcherQueryFileFormat)
        {
            if (matcherQueryFileFormat == FileFormat.OpenExcel)
                return ExcelReaderFactory.CreateOpenXmlReader(fileStream);
            else return ExcelReaderFactory.CreateCsvReader(fileStream);
        }

        private static void SelectSheetOnReader(string sheetName, IExcelDataReader reader)
        {
            for (var i = 0; i < reader.ResultsCount; i++)
            {
                if (reader.Name == sheetName)
                    break;
                reader.NextResult();
            }
        }

        private static List<string> GetAllSheetNames(IExcelDataReader reader)
        {
            var worksheetNames = new List<string>();
            for (var i = 0; i < reader.ResultsCount; i++)
            {
                worksheetNames.Add(reader.Name);
                reader.NextResult();
            }
            reader.Reset();
            return worksheetNames;
        }

        private static void EnsureSheetExists(string sheetName, IExcelDataReader excel)
        {
            if (GetAllSheetNames(excel).All(x => x != sheetName))
                throw new SheetNotFoundException(sheetName);
        }

        public Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery)
        {
            if (matcherQuery.FileFormat == null)
                matcherQuery.FileFormat = FileFormat.OpenExcel;
            if (matcherQuery.FileStream == null)
                return GetColumns(matcherQuery.FilePath, matcherQuery.Sheet, matcherQuery.FileFormat.Value);

            return GetColumns(matcherQuery.FileStream, matcherQuery.Sheet, matcherQuery.FileFormat.Value);
        }

        private async Task<IList<string>> GetColumns(string filePath, string sheetName, FileFormat fileFormat)
        {
            using (var stream = new FileInfo(filePath).OpenRead())
            {
                return await GetColumns(stream, sheetName, fileFormat);
            }
        }

        private async Task<IList<string>> GetColumns(Stream fileStream, string sheetName,
            FileFormat fileFormat)
        {
            var colNames = await Task.Run(() =>
            {
                var reader = GetReader(fileStream, fileFormat);

                if (fileFormat == FileFormat.OpenExcel)
                {
                    EnsureSheetExists(sheetName, reader);
                    SelectSheetOnReader(sheetName, reader);
                }

                return GetColumnsFromPreSelectedSheet(reader);
            });

            return colNames;
        }

        private static List<string> GetColumnsFromPreSelectedSheet(IDataReader reader)
        {
            var headerNames = new List<string>();

            reader.Read();
            var record = (IDataRecord)reader;
            for (var field = 0; field < record.FieldCount; field++)
            {
                headerNames.Add(record[field].ToString());
            }

            return headerNames;
        }
    }
}