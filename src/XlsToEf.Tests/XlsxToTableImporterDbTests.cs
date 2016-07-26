using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shouldly;
using XlsToEf.Import;
using XlsToEf.Tests.ImportHelperFiles;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests
{
    public class XlsxToTableImporterDbTests : DbTestBase
    {
        public async Task Should_Import_Column_data_into_db_from_excel()
        {
           
            var objectToUpdate = new Order
            {  
                Id = 346,
                OrderDate = DateTime.Today,
            };
            PersistToDatabase(objectToUpdate);

            var excelIoWrapper = new FakeExcelIo();
            var importer = new IdDefaultImporter(new XlsxToTableImporter(GetDb(), excelIoWrapper));
            var importMatchingData = new ImportMatchingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"Id", "xlsCol5"},
                        {"OrderDate", "xlsCol2"},
                        {"DeliveryDate", "xlsCol4"},
                    }
            };
            await importer.ImportColumnData<Order, int>(importMatchingData);

            var updatedItem = GetDb().Set<Order>().First();
            updatedItem.OrderDate.ShouldBe(new DateTime(2014, 8, 15));
            updatedItem.DeliveryDate.ShouldBe(new DateTime(2015, 9, 22));
        }

        public async Task Should_Import_new_Column_data_into_db_from_excel()
        {

            var excelIoWrapper = new FakeExcelIo();
            var importer = new IdDefaultWithCreateImporter(new XlsxToTableImporter(GetDb(), excelIoWrapper));
            var importMatchingData = new ImportMatchingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"Id", "xlsCol5"},
                        {"OrderDate", "xlsCol2"},
                        {"DeliveryDate", "xlsCol4"},
                    }
            };
            await importer.ImportColumnData<Order, int>(importMatchingData);

            var updatedItem = GetDb().Set<Order>().First();
            updatedItem.OrderDate.ShouldBe(new DateTime(2014, 8, 15));
            updatedItem.DeliveryDate.ShouldBe(new DateTime(2015, 9, 22));
        }

        public async Task Should_report_bad_data_and_process_good_data()
        {

            var objectToUpdate = new Order
            {
                Id = 346,
                OrderDate = new DateTime(2009, 1, 5),
                DeliveryDate = new DateTime(2010, 5, 7)
            };

            PersistToDatabase(objectToUpdate);
            var excelIoWrapper = new FakeExcelIo();
            var badRowIdDoesNotExist = new Dictionary<string, string>
            {
                {"xlsCol5", "999"},
                {"xlsCol2", "12/16/2016"},
                {"xlsCol4", "8/1/2014"}
            };
            excelIoWrapper.Rows.Add(badRowIdDoesNotExist);

            var importer = new IdDefaultImporter(new XlsxToTableImporter(GetDb(), excelIoWrapper));
            var importMatchingData = new ImportMatchingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"Id", "xlsCol5"},
                        {"OrderDate", "xlsCol2"},
                        {"DeliveryDate", "xlsCol4"},
                    }
            };
            var results = await importer.ImportColumnData<Order, int>(importMatchingData);

            results.SuccessCount.ShouldBe(1);
            results.RowErrorDetails.Count().ShouldBe(1);
        }

        public async Task Should_Import_Column_data_matching_nullable_column_without_error()
        {
            var objectToUpdate = new Order
            {
                Id = 346,
                OrderDate = DateTime.Today,
                DeliveryDate = null,
            };
            PersistToDatabase(objectToUpdate);

            var excelIoWrapper = new FakeExcelIo();
            var importer = new IdDefaultImporter(new XlsxToTableImporter(GetDb(), excelIoWrapper));
            var importMatchingData = new ImportMatchingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"Id", "xlsCol5"},
                        {"DeliveryDate", "xlsCol2"},
                    }
            };
            await importer.ImportColumnData<Order, int>(importMatchingData);

            var updatedItem = GetDb().Set<Order>().First();
            updatedItem.DeliveryDate.ShouldBe(new DateTime(2014, 8, 15));
        }

        public async Task Should_Import_rows_using_non_id_column()
        {
            var addressLine1 = "111 Oak Street";
            var objectToUpdate = new Address
            {
                AddrId = "123456",
                AddressLine1 = addressLine1
            };
            PersistToDatabase(objectToUpdate);

            var excelIoWrapper = new FakeExcelIo();
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
            var importMatchingData = new ImportMatchingAddressData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"Addr 1", "xlsCol6"},
                        {"AddressLine1", "xlsCol2"},
                    }
            };
            Func<string, Expression<Func<Address, bool>>> selectorFinder = (y) => z => z.AddrId == y;
            await importer.ImportColumnData(importMatchingData, finder: selectorFinder, selectorColName: "Addr 1");

            var updatedItem = GetDb().Set<Address>().First();
            updatedItem.AddressLine1.ShouldBe("8/15/2014");   
        }
    }
}