using System.Collections.Generic;

namespace XlsToEf.Import
{
    public class ImportColumnData
    {
        public ImportColumnData()
        {
            RequiredTogether = new string[0][];
        }

        public string[] XlsxColumns { get; set; }
        public Dictionary<string, SingleColumnData> TableColumns { get; set; }
        public string FileName { get; set; }
        public string[][] RequiredTogether { get; set; }
    }

    public class SingleColumnData
    {
        public SingleColumnData(string name, bool required = true)
        {
            Name = name;
            Required = required;
        }

        public string Name { get; private set; }
        public bool Required { get; private set; }
    }
}