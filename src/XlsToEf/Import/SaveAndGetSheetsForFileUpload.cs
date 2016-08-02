using System.IO;
using MediatR;

namespace XlsToEf.Import
{
    public class SaveAndGetSheetsForFileUpload : IAsyncRequest<SheetPickerInformation>
    {
        public Stream File { get; set; }
    }
}