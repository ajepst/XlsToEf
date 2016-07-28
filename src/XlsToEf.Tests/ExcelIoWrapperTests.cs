using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;

namespace XlsToEf.Tests
{
    public class ExcelIoWrapperTests
    {
        public async Task ShouldGetFromSpreadsheet()
        {
            var excel = new ExcelIoWrapper();
            var rows = await excel.GetRows("TestExcelDoc.xlsx", "Sheet2");
            rows.Count.ShouldBe(3);
            var persianRow = rows.Single(x => x["Cat"] == "Persian");
            persianRow["Quantity"].ShouldBe("2");
        }
    }
}