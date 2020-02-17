using System.IO;
using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;

namespace XlsToEf.Tests
{
    public class SheetsGetterFromFileTests
    {
        public async Task Should_Get_SheetPicker_Info()
        {
            var excelIoWrapper = new FakeExcelIo();
            var sheetsGetterFromFile = new SheetsGetterFromFile(excelIoWrapper, new FakeXlsxFileCreator());

            var result = await sheetsGetterFromFile.Handle(Stream.Null);

            result.File.ShouldBe(FakeXlsxFileCreator.FileName);
            result.Sheets.ShouldBe(excelIoWrapper.Sheets);

        }

        public async Task Should_Get_SheetPicker_Info_using_static_file()
        {
            var excelIoWrapper = new FakeExcelIo();
            var sheetsGetterFromFile = new SheetsGetterFromFile(excelIoWrapper, new FakeXlsxFileCreator());

            var fileName = @"path.xlsx";
            var result = await sheetsGetterFromFile.Handle(@"c:\this\" + fileName);

            result.File.ShouldBe(fileName);
            result.Sheets.ShouldBe(excelIoWrapper.Sheets);

        }
    }
}