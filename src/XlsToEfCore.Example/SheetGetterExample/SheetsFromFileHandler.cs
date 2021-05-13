using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using XlsToEfCore.Import;

namespace XlsToEfCore.Example.SheetGetterExample
{
    public class SheetsFromFileHandler : IRequestHandler<SaveAndGetSheetsForFileUpload, SheetPickerInformation>
    {
        private readonly SheetsGetterFromFile _getter;

        public SheetsFromFileHandler(SheetsGetterFromFile getter)
        {
            _getter = getter;
        }

        public Task<SheetPickerInformation> Handle(SaveAndGetSheetsForFileUpload uploadStream, CancellationToken cancellationToken)
        {
            var fileExtension = uploadStream.FileExtension;
            var fileFormat = fileExtension == ".xlsx" ? FileFormat.OpenExcel : FileFormat.Csv;
            return _getter.Handle(uploadStream.File, fileFormat);
        }
    }

    public class SaveAndGetSheetsForFileUpload : IRequest<SheetPickerInformation>
    {
        public Stream File { get; set; }
        public string FileExtension { get; set; }
    }
}