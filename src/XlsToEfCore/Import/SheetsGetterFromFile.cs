using System.IO;
using System.Threading.Tasks;

namespace XlsToEfCore.Import
{
    public class SheetsGetterFromFile
    {

        private readonly IExcelIoWrapper _excelIoWrapper;
        private readonly IXlsxFileCreator _xlsxFileCreator;

        public SheetsGetterFromFile(IExcelIoWrapper excelIoWrapper, IXlsxFileCreator xlsxFileCreator)
        {
            _excelIoWrapper = excelIoWrapper;
            _xlsxFileCreator = xlsxFileCreator;
        }

        public SheetsGetterFromFile()
        {
            //_excelIoWrapper = new ExcelIoWrapper();
            _excelIoWrapper = new ExcelIoAlternateWrapper();
            _xlsxFileCreator = new XlsxFileCreator();
        }

        public async Task<SheetPickerInformation> Handle(Stream uploadStream, FileFormat fileFormat)
        {
            var filePath = await _xlsxFileCreator.Create(uploadStream);
            var sheets = await _excelIoWrapper.GetSheets(filePath, fileFormat);
            return new SheetPickerInformation {Sheets = sheets, File = Path.GetFileName(filePath) };
        }
    }
}