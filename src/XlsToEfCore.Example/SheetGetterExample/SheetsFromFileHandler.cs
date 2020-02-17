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
            return _getter.Handle(uploadStream.File);
        }
    }

    public class SaveAndGetSheetsForFileUpload : IRequest<SheetPickerInformation>
    {
        public Stream File { get; set; }
    }
}