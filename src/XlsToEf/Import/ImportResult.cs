using System.Collections.Generic;

namespace XlsToEf.Import
{
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> RowErrorDetails { get; set; }
    }
}