using System.IO;
using System.Threading.Tasks;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Tests
{
    public class FakeXlsxFileCreator : IXlsxFileCreator
    {
        public const string FileName = "somefile.xlsx";
        private const string Path = @"c:\foo\";

        public Task<string> Create(Stream uploadStream)
        {
            return Task.FromResult(Path + FileName);
        }
    }
}