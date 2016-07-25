using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;
using XlsToEf.Tests;

namespace XlsToEf.Tests
{
    public class ExcelIoWrapperTests
    {
        public void ShouldGetTwoColumnSliceFromSpreadsheet()
        {
            var excel = new ExcelIoWrapper();
            var task = excel.GetFirstTwoColsSheetSlice("TestExcelDoc.xlsx", "Sheet2");
            Task.WaitAll(task);
            task.Result.Keys.Count.ShouldBe(2);
            task.Result["Price"].ShouldBe("11");
            task.Result["Count"].ShouldBe("15");
        }
    }
}