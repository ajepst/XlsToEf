using System.Collections.Generic;
using System.Threading.Tasks;
using XlsToEf.Import;

namespace XlsToEf.Tests
{
    public class FakeExcelIo : IExcelIoWrapper
    {
        public IList<string> Sheets = new List<string> {"sheet1", "sheet2"};

        public List<Dictionary<string, string>> Rows = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string>
            {
                {"xlsCol5", "346"},
                {"xlsCol1", "5643.7"},
                {"xlsCol2", "8/15/2014"},
                {"xlsCol3", "8888.5"},
                {"xlsCol4", "9/22/2015"},
                {"xlsCol6", "123456"},
            }
        };

        public Task<IList<string>> GetSheets(string filePath)
        {
            return Task.FromResult(Sheets);
        }

        public Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName)
        {
            return Task.FromResult(Rows);
        }

        public Task<IList<string>> GetColumns(string filePath, string sheet)
        {
            IList<string> result = new List<string> {"xlsCol1", "xlsCol2", "xlsCol3", "xlsCol4", "xlsCol5", "xlsCol6", "xlsCol7"};
            return Task.FromResult(result);
        }

        public Task<Dictionary<string, string>> GetFirstTwoColsSheetSlice(string filePath, string sheetName)
        {
            throw new System.NotImplementedException();
        }
    }
}