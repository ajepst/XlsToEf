# XlsToEF

[![Build status](https://ci.appveyor.com/api/projects/status/lccta2lyiq25h5i4?svg=true)](https://ci.appveyor.com/project/headspring-internal/xlstoef)

### What is XlsToEf? ###

XlsToEf is a library you can use to help you import rows from excel AND csv files and then save right to the database with Entity Framework.  It includes components to take care of most of the mechanical work of an import, and also includes several helper functions that you can use in your UI.

### What is XlsToEfCore? ###

XlsToEfCore is a version of XlsToEf set up to use with EF Core. Usage is the same, but as EF Core is a completely different dependency that works a bit differently from the original EF, there is a different Nuget. In usage, all of the below also applies to XlsToEfCore.

### Where can I get it? ###

It's available on nuget at https://www.nuget.org/packages/XlsToEf and https://www.nuget.org/packages/XlsToEfCore and you can install it from the package manager console:

```
PM> Install-package XlsToEF
```

### How Do I Get Started? (Basic usage) ###

The core of doing the import happens through the XlsxToTableImporter class, which depends on EF's DbContext class - (you can send it your own wrapped version of it if you like, so long as it inherits from the standard EF DbContext). The key method on there you're going to want to call is ImportColumnData. You must pass it at least one thing:

* Information that specifies the spreadsheet file location and the sheet you want to import

Take a look at the Example project- *ImportOrderMatchesFromXlsx* is an end-to-end example of how you would use it for each entity type you want to import into.

This is how you would do an import, in the most basic usage with all the defaults. This should work for your basic field imports:
```
var importer = new XlsxToTableImporter(myDbContext);

var importMatchingData = new DataMatchesForImport
{
    FileName = "c:\foo.xlsx",               // path to the uploaded file
    Sheet = "Sheet 2",                      // sheet in the excel doc you want to import
    FileFormat = FileFormat.OpenExcel,      // flag to indicate file format. If null, will look at the file extension, 
                                            //   but will default to excel if unknown (as with a stream).
    Selected =                              // entity fields (or just a placeholder for the field, if you use  
        new List<XlsToEfColumnPair>         //   the custom method below) mapping to the columns in the spreadsheet
        {
            new XlsToEfColumnPair{EfName="Id", XlsName="xlsCol5"},
            new XlsToEfColumnPair{EfName="ProductCategory", XlsName="xlsCol1"},
            new XlsToEfColumnPair{EfName="ProductName", XlsName="xlsCol2"},
        }
};

// does the mapping, returns success, or information about failures
return await _xlsxToTableImporter.ImportColumnData<Order>(importMatchingData); 

```
The *EfName* above is the destination field name in your EF entity, and the *XlsName* is the source column in your excel sheet. The "magic" string key as shown above going to what you'll use when the structure is being built client side using a matching UI and bound to your controller parameter (the EFName string wouldn't actually be magic as it would be generated earlier via the DataForMatcherUi.TableColumns collection and sent to the UI, dicussed below in the Additional Tools section) However if you are implementing a backend-only import with no user input, then you may be handcoding the Selected collection.  In that case, I'd avoid the "magic strings" by using an expression: 

```
var cat = new ProductCategory();
...
     Selected =
        new List<XlsToEfColumnPair>    
        {
            XlsToEfColumnPair.Create(() => cat.Id), "xlsCol5"},
            XlsToEfColumnPair.Create(() => ProductCategory, "xlsCol1"},
            XlsToEfColumnPair.Create(() => ProductName, "xlsCol2"},
        }
```

### Using Streams ###
In some cases you may not want (or have) access to the filesystem in order to read the file.  In these cases you can use the overload that supports a System.IO.Stream instance in place of a file path.

```
using(var stream = new FileInfo( "c:\foo.xlsx"))
{
	var importMatchingData = new DataMatchesForImport
	{
		FileStream = stream,               // an open file stream
		...
	};
}
```

*Note:* If you supply an instance to FileStream, the FilePath value will be ignored.

### Advanced Usage ###

If you have a more complicated scenario than just simple fields going into an entity (for instance you have to do some lookups or need to do some manual data modification) then you'll want to use the optional advanced features of XlsToEf. *ImportProductsFromXlsx* in the example project is an example of advanced usage. It uses a custom mapping overrider, *ProductPropertyOverrider*.

Here's a snippet that uses many of the advanced features:

```
// The importMatchData is the same as in the basic snippet above

// finder: maybe you want to check for a existing record/locate for updating by something other than the object Id, or maybe 
// you want to concatenate two columns to match against your selector key. Implementing your own find expression lets 
// you do that. In this example, all the database Ids need a "Z" appended to get matched.  It is run via EF, so 
// keep that in mind-not all C# code will work in here.
Func<int, Expression<Func<Product, bool>>> finderExpression = xlsValue => prod => prod.Id == xlsValue + "Z";

// See a simple implementation of an overrider in the example project, and PLEASE see the notes for implementation in the section below for fully implementing
var _productOverrider = new ProductOverrider(myContext);

// This allows you to set a custom behavior behavior for commits and for error handling.
var saveInfo = new ImportSaveBehavior 
{
    RecordMode = RecordMode.CreateOnly, 
    CommitMode = CommitMode.CommitAllAtEndIfAllGoodOrRejectAll,
};

return await _xlsxToTableImporter.ImportColumnData(importMatchingData, finderExpression, 
    overridingMapper:_productOverrider, saveBehavior: saveInfo, validator: _entityValidator);
```

More on the optional arguments:

* finder - a Func that lets ImportColumnData know how to match a particular row against the database.
* idPropertyName - The name of the Xlsx column to check against the identifier of existing objects
* overridingMapper - An overrider if you want to handle the mapping yourself - for instance if you will need to update multiple entities per row or have relationships you're going to need to look up (like you have to go look up a code in another table to get the id to put in this table). NOTE: A few less-obvious things about implementing an overrider -
  * If you edit any related entities in addition to your "main" entity in your overrider, you're going to have to make sure you handle the rollback part of the save behavior yourself. The main entity is already handled, so you don't have to do anything if that's all you're modifying.  However, if you created or modified any extra entities and you got a failure before you left your overrider, you'll want to mark those additional entities as detached/unchanged as appropriate to avoid side affects.
  * Similarly, the recordMode gets passed in, so you can obey Upsert/CreateOnly/etc for any related entities you modify. Again, this is already handled for your main entity.
  * In addition, you'll also want to throw any errors as necessary as RowParseExceptions (unless you want to stop all processing), which are caught and reported per-row higher up.
* saveBehavior - A save behavior configuration object that has two items:
  * A switch to select Update Only, Create only, or Upsert behavior. Upsert behavior is the default.
  * A switch to select the commit mode. Options are AnySuccessfulOneAtATime, AnySuccessfulAtEndAsBulk, CommitAllAtEndIfAllGoodOrRejectAll, and NoCommit. AnySuccessfulAtEndAsBulk is the default.
* validator - implements interface IEntityValidator<Entity> Optional implementation written by you for your own domain validation logic. If provided, XlsToEf will run the validator's GetValidationErrors(T entity) method for each entity after popultion, and if the returned dictionary is empty, XlsToEf will save the entity. If the dictionary is not empty, XlsToEf will roll back entity changes and return an error. Returned dictionary with error details should be in the form *Key: Field Name, Value: specific field error message*.  XlsToEf will bundle up and return out error.

### Additional Tools: ###

The ExcelIoWrapper class has several useful functions that are useful in implementing a column-matching UI like in the example project:

*GetSheets* - returns the list of sheet names in the uploaded spreadsheet

*GetImportColumnData* - This returns a collection of the column names in a particular sheet in a spreadsheet.

*DataForMatcherUi* - This is the full specification of information needed by a UI for a UI - driven excel column to table column matching tool. Usage of the below would be driven by your actual UI implementation. See BuildXlsxOrderTableMatcher in the example project for full sample usage. Example Usage:

```
var columnData = new DataForMatcherUi
{
    XlsxColumns = (await _excelIoWrapper.GetImportColumnData(message)).ToArray(),
    FileName = message.FileName,
    TableColumns = new List<TableColumnConfiguration>
    {
        TableColumnConfiguration.Create(() => order.Id, new SingleColumnData("Order ID")),
        TableColumnConfiguration.Create(() => order.OrderDate, new SingleColumnData("Order Date", required: false)),
    },
    RequiredThogether = new string[0][] // this is the default, so you can leave it off
};
```

Items that can be specified are:
 - *XlsxColumns* - Can hold an array of strings to represent the excel column headers for the selected sheet. You can build this like the example above using the *GetImportColumnData* method, or just build up a hardcoded string array if your incoming spreadsheet always has the same columns.
 - *FileName* - to hold the filename so it can be available on the way back in.
 - *TableColumns* - A specification of all columns from the destaination EF - connection object that we want to map into. Contains a list of *TableColumnConfiguration* objects.
 - *RequiredTogether* - A collection of strings intended to be used by the UI to require pairs of fields for validation. Above is the empty case - you can just leave RequiredTogether completely if you don't need it. Example usage would be like the following-note the use of the optional provided reflection helper for the second property: 
 
 ```
 RequiredTogether = new[]
                {
                    new[] { "City", GetPropertyName(() => address.State) }, 
                    new[] { GetPropertyName(() => address.IsBusiness), GetPropertyName(() => address.CompanyName) }
                }
 ```
 
*TableColumnConfiguration* - Specification for the columns to be matched against. 
 - First parameter is a name or lambda to uniquely identify the column in the import. If using the lambda, you're good. If you use the string, then you'll need to hand-map later on, using the overrider as described in the advanced section.
 -  Second parameter is the *SingleColumnData* parameter, which allows you to set the display name of the field for the UI's use, as well as whether this field should be required in your UI's validation.

