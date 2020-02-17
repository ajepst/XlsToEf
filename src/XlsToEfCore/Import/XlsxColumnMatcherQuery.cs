namespace XlsToEfCore.Import
{
    using System.IO;

    public class XlsxColumnMatcherQuery
    {
        public string FileName  { get; set; }
        public string FilePath  { get; set; }
        public string Sheet  { get; set; }
        public Stream FileStream { get; set; }
    }
}