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
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
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
            await importer.ImportColumnData<Order>(importMatchingData, recordMode:RecordMode.Upsert);

            var updatedItem = GetDb().Set<Order>().First();
            updatedItem.OrderDate.ShouldBe(new DateTime(2014, 8, 15));
            updatedItem.DeliveryDate.ShouldBe(new DateTime(2015, 9, 22));
        }

        public async Task Should_Import_new_Column_data_into_db_from_excel()
        {

            var excelIoWrapper = new FakeExcelIo();
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
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
            await importer.ImportColumnData<Order>(importMatchingData, recordMode: RecordMode.Upsert);

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

            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
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
            var results = await importer.ImportColumnData<Order>(importMatchingData, recordMode: RecordMode.CreateOnly);

            results.SuccessCount.ShouldBe(1);
            results.RowErrorDetails.Count.ShouldBe(1);
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
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
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
            await importer.ImportColumnData<Order>(importMatchingData);

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
                        {"AddrId", "xlsCol6"},
                        {"AddressLine1", "xlsCol2"},
                    }
            };
            await importer.ImportColumnData<Address>(importMatchingData);

            var updatedItem = GetDb().Set<Address>().First();
            updatedItem.AddressLine1.ShouldBe("8/15/2014");   
            updatedItem.AddrId.ShouldBe("123456");   
        }

        public async Task Should_Import_new_rows_with_generated_id_entity_createonly()
        {
            var excelIoWrapper = new FakeExcelIo();
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
            var importMatchingData = new ImportMatchingProductData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"CategoryCode", "xlsCol8"},
                        {"CategoryName", "xlsCol7"},
                    }
            };
            Func<string, Expression<Func<ProductCategory, bool>>> selectorFinder = (y) => z => z.Id == int.Parse(y);
            await importer.ImportColumnData(importMatchingData, finder: selectorFinder, recordMode:RecordMode.CreateOnly);

            var updatedItem = GetDb().Set<ProductCategory>().First();
            updatedItem.CategoryCode.ShouldBe("FRZ");
            updatedItem.CategoryName.ShouldBe("Frozen Food");
        }

        public async Task Should_Import_new_and_update_rows_with_generated_id_entity_upsert()
        {
            var objectToUpdate = new ProductCategory
            {
                CategoryCode = "AAA",
                CategoryName = "BBB"
            };
            PersistToDatabase(objectToUpdate);

            var excelIoWrapper = new FakeExcelIo();
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
            var importMatchingData = new ImportMatchingProductData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"Id", "xlsCol6"},
                        {"CategoryCode", "xlsCol8"},
                        {"CategoryName", "xlsCol7"},
                    }
            };
            var id = objectToUpdate.Id;
            excelIoWrapper.Rows[0]["xlsCol6"] = id.ToString(); // change the id to the autogenerated one so we can update it.
            excelIoWrapper.Rows.Add(
                new Dictionary<string, string>
                {
                    {"xlsCol5", "347"},
                    {"xlsCol1", "56493.7"},
                    {"xlsCol2", "8/16/2014"},
                    {"xlsCol3", "8888.5"},
                    {"xlsCol4", "9/27/2015"},
                    {"xlsCol6", ""},
                    {"xlsCol7", "Vegetables"},
                    {"xlsCol8", "VEG"},
                });

            Func<string, Expression<Func<ProductCategory, bool>>> selectorFinder = (y) => z => z.Id == int.Parse(y);
            await importer.ImportColumnData<ProductCategory>(importMatchingData);

            var updatedItem = GetDb().Set<ProductCategory>().First(x => x.Id == id);
            updatedItem.CategoryCode.ShouldBe("FRZ");
            updatedItem.CategoryName.ShouldBe("Frozen Food");

            var newItem = GetDb().Set<ProductCategory>().First(x => x.CategoryCode == "VEG");
            newItem.CategoryName.ShouldBe("Vegetables");
        }

        public async Task Should_update_rows_with_generated_id_entity_update()
        {
            var objectToUpdate = new ProductCategory
            {
                CategoryCode = "AAA",
                CategoryName = "BBB"
            };
            PersistToDatabase(objectToUpdate);

            var excelIoWrapper = new FakeExcelIo();
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
            var importMatchingData = new ImportMatchingProductData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected =
                    new Dictionary<string, string>
                    {
                        {"Id", "xlsCol6"},
                        {"CategoryCode", "xlsCol8"},
                        {"CategoryName", "xlsCol7"},
                    }
            };
            var id = objectToUpdate.Id;
            excelIoWrapper.Rows[0]["xlsCol6"] = id.ToString(); // change the id to the autogenerated one so we can update it.

            await importer.ImportColumnData<ProductCategory>(importMatchingData, recordMode: RecordMode.UpdateOnly);

            var updatedItem = GetDb().Set<ProductCategory>().First(x => x.Id == id);
            updatedItem.CategoryCode.ShouldBe("FRZ");
            updatedItem.CategoryName.ShouldBe("Frozen Food");
        }
    }
}