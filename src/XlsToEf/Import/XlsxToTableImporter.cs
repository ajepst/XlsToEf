using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace XlsToEf.Import
{
    public class XlsxToTableImporter
    {
        private readonly DbContext _dbContext;
        private readonly IExcelIoWrapper _excelIoWrapper;

        public XlsxToTableImporter(DbContext dbContext, IExcelIoWrapper excelIoWrapper)
        {
            _dbContext = dbContext;
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<ImportResult> ImportColumnData<TEntity, TSelector>(ImportMatchingData matchingData, Func<TSelector, Expression<Func<TEntity, bool>>> finder, string selectorPropertyName = null, UpdatePropertyOverrider<TEntity> overrider = null, RecordMode recordMode = RecordMode.Upsert)
           where TEntity : class, new()
        {
            var shouldSkipIdInsert = true;

            if ((recordMode == RecordMode.UpdateOnly || recordMode == RecordMode.Upsert ) && selectorPropertyName == null)
            {
                throw new Exception("Selector Column Name Required for Updates");
            }

            if ((recordMode == RecordMode.CreateOnly || recordMode == RecordMode.Upsert) && selectorPropertyName != null)
            {
                shouldSkipIdInsert = IsIdAutoIncrementing(typeof(TEntity));
                if (shouldSkipIdInsert && recordMode == RecordMode.CreateOnly)
                {
                    throw new Exception("Id is created in the database. You cannot import an ID column when creating.");
                }
            }

            var importResult = new ImportResult {RowErrorDetails = new Dictionary<string, string>()};

            var filePath  = Path.GetTempPath() + matchingData.FileName;

            var excelRows = await _excelIoWrapper.GetRows(filePath, matchingData.Sheet);
            for (var index = 0; index < excelRows.Count; index++)
            {
                var excelRow = excelRows[index];
                var rowNumber = index + 2; // add 2 to reach the first data row because the first row is a header, excel row numbers start with 1 not 0
                try
                {
                    if (excelRow.All(x => string.IsNullOrWhiteSpace(x.Value)))
                        continue;


                    TEntity matchedDbObject = null;
                    string idValue = null;
                    if (selectorPropertyName != null)
                    {
                        var xlsxSelectorColName = matchingData.Selected[selectorPropertyName];
                        var rowLabel = xlsxSelectorColName;

                        idValue = excelRow[xlsxSelectorColName];
                        if (!string.IsNullOrWhiteSpace(idValue))
                        {
                            var selectorData = (TSelector) Convert.ChangeType(idValue, typeof (TSelector));

                            var getExp = finder(selectorData);

                            matchedDbObject = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(getExp);
                            if (matchedDbObject == null && recordMode == RecordMode.UpdateOnly)
                            {
                                importResult.RowErrorDetails.Add(rowNumber.ToString(),
                                    rowLabel + " value " + selectorData +
                                    " cannot be updated - not found in database");
                                continue;
                            }

                            if (matchedDbObject != null && recordMode == RecordMode.CreateOnly)
                            {
                                importResult.RowErrorDetails.Add(rowNumber.ToString(),
                                    rowLabel + " value " + selectorData +
                                    " cannot be added - already in database");
                                continue;
                            }
                        }
                    }

                    if (matchedDbObject == null)
                    {
                        if (shouldSkipIdInsert && !string.IsNullOrWhiteSpace(idValue))
                        {
                            importResult.RowErrorDetails.Add(rowNumber.ToString(), selectorPropertyName + " value " + idValue +
                                                                                   " cannot be added - you cannot import id for new items when underlying table id is autoincrementing");
                            continue;
                        }
                        matchedDbObject = new TEntity();
                        _dbContext.Set<TEntity>().Add(matchedDbObject);
                    }


                    if (overrider != null)
                    {
                        await overrider.UpdateProperties(matchedDbObject, matchingData.Selected, excelRow);
                    }
                    else
                    {
                        UpdateProperties(matchedDbObject, matchingData.Selected, excelRow, selectorPropertyName, shouldSkipIdInsert);
                    }
                    importResult.SuccessCount++;
                }
                catch (RowParseException e)
                {
                    importResult.RowErrorDetails.Add(rowNumber.ToString(), "Error: " + e.Message);
                }
                catch (Exception e)
                {
                    importResult.RowErrorDetails.Add(rowNumber.ToString(), "Cannot be updated - error importing");
                }
            }
            _dbContext.SaveChanges();
            
            return importResult;
        }

        private bool IsIdAutoIncrementing(Type eType)
        {
            var metadata = ((IObjectContextAdapter)_dbContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == eType);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var table = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            var key = table.ElementType.KeyMembers.First();
            return key.IsStoreGeneratedIdentity;
        }

        private static void UpdateProperties<TSelector>(TSelector matchedObject, Dictionary<string, string> matches,
            Dictionary<string, string> excelRow, string selectorColName, bool shouldSkipIdInsert) where TSelector : class
        {
            foreach (var entityPropertyName in matches.Keys)
            {
                if ((entityPropertyName == selectorColName) && shouldSkipIdInsert) continue;
                var xlsxColumnName = matches[entityPropertyName];
                var xlsxItemData = excelRow[xlsxColumnName];


                Type matchedObjectType = matchedObject.GetType();
                PropertyInfo propToSet = matchedObjectType.GetProperty(entityPropertyName);

                var converted = StringToTypeConverter.Convert(xlsxItemData, propToSet.PropertyType);

                propToSet.SetValue(matchedObject, converted, null);
            }
        }
    }

    public abstract class UpdatePropertyOverrider<TSelector>
    {
        public abstract Task UpdateProperties(TSelector destination, Dictionary<string, string> destinationProperty, Dictionary<string, string> value);
    }

    public class StringToTypeConverter
    {
        public static object Convert(string xlsxItemData, Type propertyType)
        {
            object converted;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                converted = String.IsNullOrEmpty(xlsxItemData)
                    ? null :  ConvertString(xlsxItemData, propertyType.GetGenericArguments()[0]);
            }
            else
            {
                converted = ConvertString(xlsxItemData, propertyType);
            }
            return converted;
        }

        private static object ConvertString(string xlsxItemData, Type propertyType)
        {
            if (propertyType == typeof (short))
                return short.Parse(xlsxItemData, NumberStyles.AllowThousands);

            if (propertyType == typeof(int))
                return int.Parse(xlsxItemData, NumberStyles.AllowThousands);

            if (propertyType == typeof(byte))
                return byte.Parse(xlsxItemData, NumberStyles.AllowThousands);
//            if (propertyType == typeof (bool))
//                return DisplayConversions.StringToBool(xlsxItemData);

            return System.Convert.ChangeType(xlsxItemData, propertyType, CultureInfo.CurrentCulture);
        }
    }


    public enum RecordMode
    {
        UpdateOnly,
        CreateOnly,
        Upsert
    }
}