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
            var order = new Order();
            var importMatchingData = new DataMatchesForImportingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => order.Id, "xlsCol5"),
                    XlsToEfColumnPair.Create("OrderDate", "xlsCol2"),
                    XlsToEfColumnPair.Create(() => order.DeliveryDate, "xlsCol4"),

                },
            };
            await importer.ImportColumnData<Order>(importMatchingData);

            var updatedItem = GetDb().Set<Order>().First();
            updatedItem.OrderDate.ShouldBe(new DateTime(2014, 8, 15));
            updatedItem.DeliveryDate.ShouldBe(new DateTime(2015, 9, 22));
        }

        public async Task Should_Import_new_Column_data_into_db_from_excel()
        {

            var excelIoWrapper = new FakeExcelIo();
            var importer = new XlsxToTableImporter(GetDb(), excelIoWrapper);
            var order = new Order();
            var importMatchingData = new DataMatchesForImportingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => order.Id, "xlsCol5"),
                    XlsToEfColumnPair.Create("OrderDate", "xlsCol2"),
                    XlsToEfColumnPair.Create(() => order.DeliveryDate, "xlsCol4"),

                },
            };
            await importer.ImportColumnData<Order>(importMatchingData, saveBehavior:new ImportSaveBehavior { RecordMode = RecordMode.Upsert});

            var updatedItem = GetDb().Set<Order>().First();
            updatedItem.OrderDate.ShouldBe(new DateTime(2014, 8, 15));
            updatedItem.DeliveryDate.ShouldBe(new DateTime(2015, 9, 22));
        }

        public async Task Should_report_bad_data_and_save_good_data_with_only_updates_allowed()
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

            var dbContext = GetDb();
            var importer = new XlsxToTableImporter(dbContext, excelIoWrapper);
            var order = new Order();
            var importMatchingData = new DataMatchesForImportingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => order.Id, "xlsCol5"),
                    XlsToEfColumnPair.Create("OrderDate", "xlsCol2"),
                    XlsToEfColumnPair.Create(() => order.DeliveryDate, "xlsCol4"),

                },
            };
            var results = await importer.ImportColumnData<Order>(importMatchingData, new ImportSaveBehavior { RecordMode= RecordMode.UpdateOnly});

            results.SuccessCount.ShouldBe(1);
            results.RowErrorDetails.Count.ShouldBe(1);

            var dbSet = dbContext.Set<Order>().ToArray();
            dbSet.Length.ShouldBe(1);
            var entity = dbSet.First();
            entity.DeliveryDate.ShouldBe(DateTime.Parse("9/22/2015"));
        }

        public async Task Should_report_bad_data_and_save_good_data_with_only_updates_allowed_incremental_saves()
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

            var dbContext = GetDb();
            var importer = new XlsxToTableImporter(dbContext, excelIoWrapper);
            var order = new Order();
            var importMatchingData = new DataMatchesForImportingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => order.Id, "xlsCol5"),
                    XlsToEfColumnPair.Create("OrderDate", "xlsCol2"),
                    XlsToEfColumnPair.Create(() => order.DeliveryDate, "xlsCol4"),

                },
            };
            var results =
                await
                    importer.ImportColumnData<Order>(importMatchingData,
                        new ImportSaveBehavior
                        {
                            RecordMode = RecordMode.UpdateOnly,
                            CommitMode = CommitMode.AnySuccessfulOneAtATime
                        });

            results.SuccessCount.ShouldBe(1);
            results.RowErrorDetails.Count.ShouldBe(1);

            var dbSet = dbContext.Set<Order>().ToArray();
            dbSet.Length.ShouldBe(1);
            var entity = dbSet.First();
            entity.DeliveryDate.ShouldBe(DateTime.Parse("9/22/2015"));
        }

        public async Task Should_reject_all_changes_if_all_or_nothing_and_encounters_error()
        {
            var originalOrderDate = new DateTime(2009, 1, 5);
            var originalDeliveryDate = new DateTime(2010, 5, 7);
            var objectToUpdate = new Order
            {
                Id = 346,
                OrderDate = originalOrderDate,
                DeliveryDate = originalDeliveryDate
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

            var dbContext = GetDb();
            var importer = new XlsxToTableImporter(dbContext, excelIoWrapper);
            var order = new Order();
            var importMatchingData = new DataMatchesForImportingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => order.Id, "xlsCol5"),
                    XlsToEfColumnPair.Create("OrderDate", "xlsCol2"),
                    XlsToEfColumnPair.Create(() => order.DeliveryDate, "xlsCol4"),

                },
            };
            var results =
                await
                    importer.ImportColumnData<Order>(importMatchingData,
                        new ImportSaveBehavior
                        {
                            RecordMode = RecordMode.CreateOnly,
                            CommitMode = CommitMode.CommitAllAtEndIfAllGoodOrRejectAll
                        });

            results.SuccessCount.ShouldBe(1);
            results.RowErrorDetails.Count.ShouldBe(1);


            var dbSet = dbContext.Set<Order>().ToArray();
            dbSet.Count().ShouldBe(1);
            dbSet
                .Any(x => x.DeliveryDate == originalDeliveryDate && x.OrderDate == originalOrderDate)
                .ShouldBe(true);
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
            var order = new Order();
            var importMatchingData = new DataMatchesForImportingOrderData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => order.Id, "xlsCol5"),
                    XlsToEfColumnPair.Create(() => order.DeliveryDate, "xlsCol2"),

                },
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
            var addr = new Address();
            var importMatchingData = new DataMatchesForImportingAddressData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => addr.AddrId, "xlsCol6"),
                    XlsToEfColumnPair.Create(() => addr.AddressLine1, "xlsCol2"),

                },
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
            var cat = new ProductCategory();
            var importMatchingData = new DataMatchesForImportingProductData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => cat.CategoryCode, "xlsCol8"),
                    XlsToEfColumnPair.Create("CategoryName", "xlsCol7"),
                },
            };
            Func<string, Expression<Func<ProductCategory, bool>>> selectorFinder = (y) => z => z.Id == int.Parse(y);
            await importer.ImportColumnData(importMatchingData, finder: selectorFinder, saveBehavior: new ImportSaveBehavior { RecordMode=RecordMode.CreateOnly});

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
            var cat = new ProductCategory();
            var importMatchingData = new DataMatchesForImportingProductData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => cat.Id, "xlsCol6"),
                    XlsToEfColumnPair.Create("CategoryCode", "xlsCol8"),
                    XlsToEfColumnPair.Create(() => cat.CategoryName, "xlsCol7"),

                },
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
            var cat = new ProductCategory();
            var importMatchingData = new DataMatchesForImportingProductData
            {
                FileName = "foo.xlsx",
                Sheet = "mysheet",
                Selected = new List<XlsToEfColumnPair>
                {
                    XlsToEfColumnPair.Create(() => cat.Id, "xlsCol6"),
                    XlsToEfColumnPair.Create("CategoryCode", "xlsCol8"),
                    XlsToEfColumnPair.Create(() => cat.CategoryName, "xlsCol7"),

                },
            };
            var id = objectToUpdate.Id;
            excelIoWrapper.Rows[0]["xlsCol6"] = id.ToString(); // change the id to the autogenerated one so we can update it.

            await importer.ImportColumnData<ProductCategory>(importMatchingData, saveBehavior: new ImportSaveBehavior { RecordMode= RecordMode.UpdateOnly});

            var updatedItem = GetDb().Set<ProductCategory>().First(x => x.Id == id);
            updatedItem.CategoryCode.ShouldBe("FRZ");
            updatedItem.CategoryName.ShouldBe("Frozen Food");
        }
    }
}