using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity">The type of EF Entity</typeparam>
        /// <param name="matchingData">Specification for how to match spreadsheet to entity</param>
        /// <param name="finder">A func to look for an existing copy of the entity id. default will use "find". Gets run via EF.</param>
        /// <param name="idPropertyName">The unique identifier property. Leave blank unless you are using an overrider to use something other than the real key.</param>
        /// <param name="overridingMapper">An custom mapper for mapping between excel columns and an entity. </param>
        /// <param name="recordMode">Indicate whether import should be UpdateOnly, CreateOnly or Upsert. Upsert is the default.</param>
        /// <returns></returns>
        public async Task<ImportResult> ImportColumnData<TEntity>(ImportMatchingData matchingData, Func<string, Expression<Func<TEntity, bool>>> finder = null, string idPropertyName = null, UpdatePropertyOverrider<TEntity> overridingMapper = null, RecordMode recordMode = RecordMode.Upsert)
           where TEntity : class, new()
        {
            var keyInfo =  GetEntityKeys(typeof (TEntity));
            EnsureImportingEntityHasSingleKey(keyInfo);
            var pk = keyInfo[0];
            Type idType = ((PrimitiveType) pk.TypeUsage.EdmType).ClrEquivalentType;


            if (idPropertyName == null)
            {
                idPropertyName = pk.Name;
            }

            var isImportingEntityId = matchingData.Selected.ContainsKey(idPropertyName);
            var isAutoIncrementingId = IsIdAutoIncrementing(typeof(TEntity));

            EnsureNoIdColumnIncludedWhenCreatingAutoIncrementEntites(recordMode, isAutoIncrementingId, isImportingEntityId);
            
            var importResult = new ImportResult {RowErrorDetails = new Dictionary<string, string>()};

            var filePath  = Path.GetTempPath() + matchingData.FileName;

            var excelRows = await _excelIoWrapper.GetRows(filePath, matchingData.Sheet);

            for (var index = 0; index < excelRows.Count; index++)
            {
                var excelRow = excelRows[index];
                var rowNumber = index + 2; // add 2 to reach the first data row because the first row is a header, excel row numbers start with 1 not 0
                try
                {
                    if (ExcelRowIsBlank(excelRow))
                        continue;


                    TEntity entityToUpdate = null;
                    string idValue = null;
                    if (isImportingEntityId)
                    {
                        var xlsxIdColName = matchingData.Selected[idPropertyName];
                        var idStringValue = excelRow[xlsxIdColName];
                        entityToUpdate = await GetMatchedDbObject(finder, idStringValue, idType);

                        ValidateDbResult(entityToUpdate, recordMode, xlsxIdColName, idStringValue);
                    }

                    if (entityToUpdate == null)
                    {
                        EnsureNoEntityCreationWithIdWhenAutoIncrementIdType(idPropertyName, isAutoIncrementingId, idValue);
                        entityToUpdate = new TEntity();
                        _dbContext.Set<TEntity>().Add(entityToUpdate);
                    }

                    await MapIntoEntity(matchingData, idPropertyName, overridingMapper, entityToUpdate, excelRow, isAutoIncrementingId);
                    importResult.SuccessCount++;
                }
                catch (RowParseException e)
                {
                    importResult.RowErrorDetails.Add(rowNumber.ToString(), "Error: " + e.Message);
                }
                catch (Exception)
                {
                    importResult.RowErrorDetails.Add(rowNumber.ToString(), "Cannot be updated - error importing");
                }
            }
            _dbContext.SaveChanges();
            
            return importResult;
        }

        private static async Task MapIntoEntity<TEntity>(ImportMatchingData matchingData, string idPropertyName,
            UpdatePropertyOverrider<TEntity> overridingMapper, TEntity entityToUpdate, Dictionary<string, string> excelRow, bool isAutoIncrementingId)
            
        {
            if (overridingMapper != null)
            {
                await overridingMapper.UpdateProperties(entityToUpdate, matchingData.Selected, excelRow);
            }
            else
            {
                UpdateProperties(entityToUpdate, matchingData.Selected, excelRow, idPropertyName, isAutoIncrementingId);
            }
        }

        // this condition might happen when Upserting. unlike the other check, this check is per-row.
        private static void EnsureNoEntityCreationWithIdWhenAutoIncrementIdType(string idPropertyName,
            bool isAutoIncrementingId, string idValue) 
        {
            if (isAutoIncrementingId && !string.IsNullOrWhiteSpace(idValue))
            {
                throw new RowParseException(idPropertyName + " value " + idValue +
                                            " cannot be added - you cannot import id for new items when underlying table id is autoincrementing");
            }
        }

        private async Task<TEntity> GetMatchedDbObject<TEntity>(Func<string, Expression<Func<TEntity, bool>>> finder, string idStringValue, Type idType) where TEntity : class
        {
            if (string.IsNullOrWhiteSpace(idStringValue)) return null;

            TEntity matchedDbObject;
            if (finder != null)
            {
                var getExp = finder(idStringValue);
                matchedDbObject = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(getExp);
            }
            else
            {
                var idData = Convert.ChangeType(idStringValue, idType);
                matchedDbObject = await _dbContext.Set<TEntity>().FindAsync(idData);
            }

            return matchedDbObject;
        }

        private static void ValidateDbResult<TEntity>(TEntity matchedDbObject, RecordMode recordMode, string xlsxIdColName, string idValue) where TEntity : class
        {
            if (matchedDbObject == null && recordMode == RecordMode.UpdateOnly)
            {
                throw new RowParseException(xlsxIdColName + " value " + idValue +
                                            " cannot be updated - not found in database");
            }

            if (matchedDbObject != null && recordMode == RecordMode.CreateOnly)
            {
                throw new RowParseException(xlsxIdColName + " value " + idValue + " cannot be added - already in database");
            }
        }

        private static bool ExcelRowIsBlank(Dictionary<string, string> excelRow)
        {
            return excelRow.All(x => string.IsNullOrWhiteSpace(x.Value));
        }

        private static void EnsureNoIdColumnIncludedWhenCreatingAutoIncrementEntites(RecordMode recordMode,
            bool isAutoIncrementingId, bool isImportingEntityId)
        {
            if (isAutoIncrementingId && isImportingEntityId && recordMode == RecordMode.CreateOnly)
            {
                throw new Exception("Id is created in the database. You cannot import an ID column when creating.");
            }
        }

        private void EnsureImportingEntityHasSingleKey(ReadOnlyMetadataCollection<EdmMember> keyInfo) { 


            if (keyInfo.Count > 1)
            {
                throw new Exception("XlsToEf only supports Single Column Key right now");
            }
        }

        private bool IsIdAutoIncrementing(Type eType)
        {
            EdmMember key = GetMappedKeyInformation(eType);
            return key.IsStoreGeneratedIdentity;
        }

        private ReadOnlyMetadataCollection<EdmMember> GetEntityKeys(Type eType)
        {
            var metadata = ((IObjectContextAdapter)_dbContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                .GetItems<EntityType>(DataSpace.OSpace)
                .Single(e => objectItemCollection.GetClrType(e) == eType);

            // Get the entity set that uses this entity type
            
            return entityType.KeyMembers;
        }

        private EdmMember GetMappedKeyInformation(Type eType)
        {
            var metadata = ((IObjectContextAdapter) _dbContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection) metadata.GetItemCollection(DataSpace.OSpace));

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

            var readOnlyMetadataCollection = table.ElementType.KeyMembers;
            return readOnlyMetadataCollection[0];
        }

        private static void UpdateProperties<TSelector>(TSelector matchedObject, Dictionary<string, string> matches,
            Dictionary<string, string> excelRow, string selectorColName, bool shouldSkipIdInsert) 
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

    public static class StringToTypeConverter
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