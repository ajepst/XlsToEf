#XlsToEF#

[![Build Status](https://ci.appveyor.com/api/projects/status/github/ajepst/XlstoEf?branch=master&svg=true)](https://ci.appveyor.com/project/ajepst/xlstoef)

### What is XlsToEf? ###

XlsToEf is a library you can use to help you import rows from excel files and then save right to the database with Entity Framework.  It includes components to take care of most of the mechanical work of an import, and also includes several helper functions that you can use in your UI.

###How Do I Get Started?###

The core of doing the import happens through calling ImportColumnData. You must pass it at least one thing:

* Information that specifies the spreadsheet file location and the sheet you want to import

Take a look at the Example project- *ImportOrderMatchesFromXlsx* is how you would use it for each entity type you want to import into. If you have a more complicated scenario than just simple fields going into an entity (for instance you have to do some lookups or need to do some manual data changes) *ImportProductsFromXlsx*, will be a better example. It uses a custom mapping overrider, *ProductPropertyOverrider*.

Optionally, you can pass a few more things:

* a Func that lets ImportColumnData know how to match a particular row against the database.
* The name of the Xlsx column to check against the identifier of existing objects
* An overrider if you want to handle the mapping yourself - for instance if you will need to update multiple entities per row or have relationships you're going to need to look up. NOTE: A few less-obvious things about implementing an overrider -
  * If you edit any related entities in addition to your "main" entity in your overrider, you're going to have to make sure you handle the rollback part of the save behavior yourself. The main entity is already handled, so you don't have to do anything if that's all you're modifying.  However, if you created or modified any extra entities and you got a failure before you left your overrider, you'll want to mark those additional entities as detached/unchanged as appropriate to avoid side affects.
  * Similarly, the recordMode gets passed in, so you can obey Upsert/CreateOnly/etc for any related entities you modify. Again, this is already handled for your main entity.
  * In addition, you'll also want to throw any errors as necessary as RowParseExceptions (unless you want to stop all processing), which are caught and reported per-row higher up.
* A save behavior configuration object that has two items:
  * A switch to select Update Only, Create only, or Upsert behavior. Upsert behavior is the default.
  * A switch to select the commit mode. Options are AnySuccessfulOneAtATime, AnySuccessfulAtEndAsBulk, CommitAllAtEndIfAllGoodOrRejectAll, and NoCommit. AnySuccessfulAtEndAsBulk is the default.

###Additional Tools:###

The ExcelIoWrapper class has several useful functions that are useful in implementing a column-matching UI:

*GetSheets* - returns the list of sheet names in the uploaded spreadsheet

*GetImportColumnData* - This returns a collection of the column names in a spreadsheet. This could be useful for implementing a matching UI, as in the example project.

### Notable Dependencies ###

Like most open source Excel integration tools, this project relies on the Microsoft Access Database Engine 2010 Redistributable, which you can get here:
https://www.microsoft.com/en-us/download/details.aspx?id=13255
The CI build is set up for AppVeyor, and installs it automatically-the basic build does not. If you use [Chocolatey](https://chocolatey.org/) you can easily install it with the following:

```
choco install msaccess2010-redist-x64
```
