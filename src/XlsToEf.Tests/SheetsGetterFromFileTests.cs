using System.IO;
using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;
using XlsToEf.Tests;

namespace XlsToEx.Tests
{
    public class SheetsGetterFromFileTests
    {
        public async Task Should_Get_SheetPicker_Info()
        {
            var excelIoWrapper = new FakeExcelIo();
            var sheetsGetterFromFile = new SheetsGetterFromFile(excelIoWrapper, new FakeXlsxFileCreator());

            var saveAndGetSheetsForFileUpload = new SaveAndGetSheetsForFileUpload
            {
                File = Stream.Null
            };
            var result = await sheetsGetterFromFile.Handle(saveAndGetSheetsForFileUpload);

            result.File.ShouldBe(FakeXlsxFileCreator.FileName);
            result.Sheets.ShouldBe(excelIoWrapper.Sheets);

        }
    }
}