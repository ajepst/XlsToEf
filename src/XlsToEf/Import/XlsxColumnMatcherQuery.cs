namespace XlsToEf.Import
{
    public class XlsxColumnMatcherQuery
    {
        public string FileName  { get; set; }
        public string FilePath  { get; set; }
        public string Sheet  { get; set; }
        public char? Delimiter { get; set; }
    }
}