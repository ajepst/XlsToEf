using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
}