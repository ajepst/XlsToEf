using System.IO;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Import;

namespace XlsToEf.Example.SheetGetterExample
{
    public class SheetsFromFileHandler : IAsyncRequestHandler<SaveAndGetSheetsForFileUpload, SheetPickerInformation>
    {
        private readonly SheetsGetterFromFile _getter;

        public SheetsFromFileHandler(SheetsGetterFromFile getter)
        {
            _getter = getter;
        }

        public async Task<SheetPickerInformation> Handle(SaveAndGetSheetsForFileUpload uploadStream)
        {
            return await _getter.Handle(uploadStream.File);
        }
    }

    public class SaveAndGetSheetsForFileUpload : IAsyncRequest<SheetPickerInformation>
    {
        public Stream File { get; set; }
    }
}