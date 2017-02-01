using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;

namespace XlsToEf.Tests
{
    public class ExcelIoWrapperTests
    {
        public async Task ShouldGetFromXlsxSpreadsheet()
        {
            var excel = new ExcelIoWrapper();
            var cols =
                await excel.GetImportColumnData(new XlsxColumnMatcherQuery
                    {
                        FilePath = AppDomain.CurrentDomain.BaseDirectory + @"\TestExcelDoc.xlsx",
                        Sheet = "Sheet2"
                    });
            cols.Count.ShouldBe(2);
            cols[0].ShouldBe("Cat");
            cols[1].ShouldBe("Quantity");
            var rows = await excel.GetRows("TestExcelDoc.xlsx", "Sheet2");
            rows.Count.ShouldBe(3);
            var persianRow = rows.Single(x => x["Cat"] == "Persian");
            persianRow["Quantity"].ShouldBe("2");
        }

        public async Task ShouldGetFromCsvSpreadsheet()
        {
            var excel = new ExcelIoWrapper();
            var cols =
                await excel.GetImportColumnData(new XlsxColumnMatcherQuery
                {
                    FilePath = AppDomain.CurrentDomain.BaseDirectory + @"\TestCsvDoc.csv",
                    Sheet = "Sheet1",
                    // uses default delimiter, comma
                });
            cols.Count.ShouldBe(2);
            cols[0].ShouldBe("Cat");
            cols[1].ShouldBe("Quantity");
            var rows = await excel.GetRows("TestCsvDoc.csv", "Sheet1"); // uses default delimiter, comma
            rows.Count.ShouldBe(3);
            var persianRow = rows.Single(x => x["Cat"] == "Persian");
            persianRow["Quantity"].ShouldBe("2");
        }

        public async Task ShouldGetFromTabDelimitedSpreadsheet()
        {
            var excel = new ExcelIoWrapper();
            var cols =
                await excel.GetImportColumnData(new XlsxColumnMatcherQuery
                {
                    FilePath = AppDomain.CurrentDomain.BaseDirectory + @"\TestTabDoc.txt",
                    Sheet = "NotUsed",
                    Delimiter = '\t'
                });
            cols.Count.ShouldBe(2);
            cols[0].ShouldBe("Cat");
            cols[1].ShouldBe("Quantity");
            var rows = await excel.GetRows("TestTabDoc.txt", "NotUsed", delimiter: '\t');
            rows.Count.ShouldBe(4); // last row is blank/empty
            var persianRow = rows.Single(x => x["Cat"] == "Persian");
            persianRow["Quantity"].ShouldBe("2");
        }
    }
}