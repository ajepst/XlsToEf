using System.IO;
using System.Threading.Tasks;

namespace XlsToEf.Import
{
    public interface IXlsxFileCreator
    {
        Task<string> Create(Stream uploadStream);
    }

    public class XlsxFileCreator : IXlsxFileCreator
    {
        public async Task<string> Create(Stream uploadStream)
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var xlsPath = Path.ChangeExtension(path, "xlsx");
            using (var fileStream = File.Create(xlsPath))
            {
                await uploadStream.CopyToAsync(fileStream);
            }
            return xlsPath;
        }
    }
}