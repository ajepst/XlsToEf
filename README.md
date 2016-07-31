#XlsToEF#

[![Build Status](https://ci.appveyor.com/api/projects/status/github/ajepst/XlstoEf?branch=master&svg=true)](https://ci.appveyor.com/project/ajepst/xlstoef)

###What is XlsToEf?###

XlsToEf is a library you can use to help you import rows from excel files and then save right to the database with Entity Framework.  It includes components to take care of most of the mechanical work of an import, and also includes several helper functions that you can use in your UI.

### Where can I get it? ###
It's available on nuget at https://www.nuget.org/packages/XlsToEf and you can install it from the package manager console:

```
PM> Install-package XlsToEF
```

###How Do I Get Started? (Basic usage)###

The core of doing the import happens through the XlsxToTableImporter class, which depends on EF's DbContext class - (you can send it your own wrapped version of it if you like, so long as it inherits from the standard EF DbContext). The key method on there you're going to want to call is ImportColumnData. You must pass it at least one thing:

* Information that specifies the spreadsheet file location and the sheet you want to import

Take a look at the Example project- *ImportOrderMatchesFromXlsx* is an end-to-end example of how you would use it for each entity type you want to import into.

This is how you would do an import, in the most basic usage with all the defaults. This should work for your basic field imports:
```
var importer = new XlsxToTableImporter(myContext);

var importMatchingData = new ImportMatchingOrderData
{
    FileName = "c:\foo.xlsx",               // path to the uploaded file
    Sheet = "Sheet 2",                      // sheet in the excel doc you want to import
    Selected =                              // entity fields (or just a placeholder, if you use the custom  
        new Dictionary<string, string>      //     method below) mapping to the columns in the spreadsheet
        {
            {"Id", "xlsCol5"},
            {"ProductCategory", "xlsCol1"},
            {"ProductName", "xlsCol2"},
        }
};

// does the mapping, returns success, or information about failures
return await _xlsxToTableImporter.ImportColumnData<Order>(importMatchingData); 

```
Note: if you don't want to use a "magic string" for your property (which you probably shouldn't if you're not using the custom mapping method below) you can use an expression with a provided helper: 

```
var cat = new ProductCategory();
...
     Selected =
        new Dictionary<string, string>      
        {
            {PropertyNameHelper.GetPropertyName(() => cat.Id), "xlsCol5"},
            {PropertyNameHelper.GetPropertyName(() => ProductCategory, "xlsCol1"},
            {PropertyNameHelper.GetPropertyName(() => ProductName, "xlsCol2"},
        }
```

There is an existing open issue to work out a way to use a lambda without the hack of using the helper (while still allowing a string in some way for custom scenarios)... so that should improve at some point. 

###Advanced Usage###

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
    overridingMapper:_productOverrider, saveBehavior: saveInfo);
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

###Additional Tools:###

The ExcelIoWrapper class has several useful functions that are useful in implementing a column-matching UI like in the example project:

*GetSheets* - returns the list of sheet names in the uploaded spreadsheet

*GetImportColumnData* - This returns a collection of the column names in a particular sheet in a spreadsheet.

### Notable Dependencies ###

Like most open source Excel integration tools, this project relies on the Microsoft Access Database Engine 2010 Redistributable, which you can get here:
https://www.microsoft.com/en-us/download/details.aspx?id=13255
The CI build is set up for AppVeyor, and installs it automatically-the basic build does not. If you use [Chocolatey](https://chocolatey.org/) you can easily install it with the following:

```
choco install msaccess2010-redist-x64
```
