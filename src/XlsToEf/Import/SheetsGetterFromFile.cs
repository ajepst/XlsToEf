using System.IO;
using System.Threading.Tasks;
using MediatR;

namespace XlsToEf.Import
{
    public class SheetsGetterFromFile : IAsyncRequestHandler<SaveAndGetSheetsForFileUpload, SheetPickerInformation>
    {
        private readonly IExcelIoWrapper _excelIoWrapper;
        private readonly IXlsxFileCreator _xlsxFileCreator;

        public SheetsGetterFromFile(IExcelIoWrapper excelIoWrapper, IXlsxFileCreator xlsxFileCreator)
        {
            _excelIoWrapper = excelIoWrapper;
            _xlsxFileCreator = xlsxFileCreator;
        }

        public async Task<SheetPickerInformation> Handle(SaveAndGetSheetsForFileUpload uploadStream)
        {
            var filePath = await _xlsxFileCreator.Create(uploadStream.File);
            var sheets = await _excelIoWrapper.GetSheets(filePath);
            return new SheetPickerInformation {Sheets = sheets, File = Path.GetFileName(filePath) };
        }
    }
}