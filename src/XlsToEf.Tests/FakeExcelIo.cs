﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XlsToEf.Import;

namespace XlsToEf.Tests
{
    using System.IO;

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
                {"xlsCol7", "Frozen Food"},
                {"xlsCol8", "FRZ"},
            },

        };

        public Task<IList<string>> GetSheets(string filePath, FileFormat fileFormat)
        {
            return Task.FromResult(Sheets);
        }

        public Task<IList<string>> GetSheets(Stream fileStream, FileFormat fileFormat)
        {
            return Task.FromResult(Sheets);
        }

        public Task<List<Dictionary<string, string>>> GetRows(string filePath, string sheetName, FileFormat fileFormat)
        {
            FileName = filePath;
            return Task.FromResult(Rows);
        }

        public Task<List<Dictionary<string, string>>> GetRows(Stream fileStream, string sheetName, FileFormat fileFormat)
        {
            return Task.FromResult(Rows);
        }

        public Task<Dictionary<string, string>> GetFirstTwoColsSheetSlice(string filePath, string sheetName)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery)
        {
            IList<string> result = new List<string> { "xlsCol1", "xlsCol2", "xlsCol3", "xlsCol4", "xlsCol5", "xlsCol6", "xlsCol7" };
            return Task.FromResult(result);
        }

        public string FileName { get; set; }
    }
}