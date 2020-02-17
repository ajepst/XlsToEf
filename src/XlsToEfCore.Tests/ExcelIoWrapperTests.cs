using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using XlsToEfCore.Import;

namespace XlsToEfCore.Tests
{
    using System.IO;

    public class ExcelIoWrapperTests
    {
        public async Task ShouldGetFromSpreadsheet()
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

        public async Task ShouldGetFromSpreadsheetUsingStream()
        {
            var excel = new ExcelIoWrapper();
            using (var stream = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\TestExcelDoc.xlsx").OpenRead())
            {
                var cols =
                    await excel.GetImportColumnData(new XlsxColumnMatcherQuery
                    {
                        FileStream = stream,
                        Sheet = "Sheet2"
                    });

                cols.Count.ShouldBe(2);
                cols[0].ShouldBe("Cat");
                cols[1].ShouldBe("Quantity");
                var rows = await excel.GetRows(stream, "Sheet2");
                rows.Count.ShouldBe(3);
                var persianRow = rows.Single(x => x["Cat"] == "Persian");
                persianRow["Quantity"].ShouldBe("2");
            }
        }
    }
}