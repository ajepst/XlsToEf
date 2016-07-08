using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;
using XlsToEf.Tests;

namespace XlsToEx.Tests
{
    public class ExcelIoWrapperTests
    {
        [Skip]
        public void ShouldGetTwoColumnSliceFromSpreadsheet()
        {
            var excel = new ExcelIoWrapper();
            var task = excel.GetFirstTwoColsSheetSlice("TestExcelDoc.xlsx", "Sheet2");
            Task.WaitAll(task);
            task.Result.Keys.Count.ShouldBe(2);
            task.Result["Mileage Start"].ShouldBe("11");
            task.Result["Mileage Stop"].ShouldBe("15");
        }
    }
}