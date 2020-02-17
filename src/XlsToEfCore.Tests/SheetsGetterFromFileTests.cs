using System.IO;
using System.Threading.Tasks;
using Shouldly;
using XlsToEfCore.Import;

namespace XlsToEfCore.Tests
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
    }
}