using System.Collections.Generic;

namespace XlsToEf.Import
{
    public class ImportMatchingData
    {
        public string FileName { get; set; }
        public string Sheet { get; set; }
        public Dictionary<string, string> Selected { get; set; } 
    }
}