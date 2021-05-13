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

        public XlsxToTableImporter(DbContext dbContext)
        {
            _dbContext = dbContext;
            _excelIoWrapper = new ExcelIoWrapper();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity">The type of EF Entity</typeparam>
        /// <param name="matchingData">Specification for how to match spreadsheet to entity</param>
        /// <param name="saveBehavior">Optional configuration to change the save behavior. See ImportSaveBehavior</param>
        /// <param name="fileLocation">Optional directory of source xlsx file. Defaults to temp dir</param>
        /// <returns></returns>
        public async Task<ImportResult> ImportColumnData<TEntity>(DataMatchesForImport matchingData, ImportSaveBehavior saveBehavior = null, string fileLocation = null)
            where TEntity : class, new()
        {
            return await ImportColumnData<TEntity, string>(matchingData, null, null, null, saveBehavior, fileLocation:fileLocation);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity">The type of EF Entity</typeparam>
        /// <typeparam name="TId">Type of the item to use for finding</typeparam>
        /// <param name="matchingData">Specification for how to match spreadsheet to entity</param>
        /// <param name="finder">A func to look for an existing copy of the entity id. default will use "find". Runs against the DB via EF.</param>
        /// <param name="idPropertyName">The unique identifier property. Leave blank unless you are using an overrider to use something other than the real key.</param>
        /// <param name="overridingMapper">A custom mapper for mapping between excel columns and an entity. </param>
        /// <param name="saveBehavior">Optional configuration to change the save behavior. See ImportSaveBehavior</param>
        /// <param name="validator">Optional method to run custom validation on the modified entity before saving</param>
        /// <param name="fileLocation">Directory of source xlsx file. Defaults to temp dir</param>
        /// <returns></returns>
        public async Task<ImportResult> ImportColumnData<TEntity, TId>(DataMatchesForImport matchingData, Func<TId, Expression<Func<TEntity, bool>>> finder = null, string idPropertyName = null, IUpdatePropertyOverrider<TEntity> overridingMapper = null, ImportSaveBehavior saveBehavior = null, IEntityValidator<TEntity> validator = null, string fileLocation = null)
           where TEntity : class, new()
        {
            if (saveBehavior == null)
            {
                saveBehavior = new ImportSaveBehavior();
            }

            var selectedDict = BuildDictionaryFromSelected(matchingData.Selected);
            var isAutoIncrementingId = IsIdAutoIncrementing(typeof(TEntity));

            var keyInfo =  GetEntityKeys(typeof (TEntity));
            EnsureImportingEntityHasSingleKey(keyInfo);
            var pk = keyInfo[0];

            ValidateIdTypes<TEntity, TId>(idPropertyName, pk);

            var isImportingEntityId = selectedDict.ContainsKey(pk.Name);
            EnsureNoIdColumnIncludedWhenCreatingAutoIncrementEntities(saveBehavior.RecordMode, isAutoIncrementingId, isImportingEntityId);
            
            if (idPropertyName == null)
            {
                idPropertyName = pk.Name;
            }

            var isImportingMatchableEntities = selectedDict.ContainsKey(idPropertyName);

            Type trueIdType = ((PrimitiveType)pk.TypeUsage.EdmType).ClrEquivalentType;



            var importResult = new ImportResult {RowErrorDetails = new Dictionary<string, string>()};

            var excelRows = await GetExcelRows(matchingData, fileLocation);

            var foundErrors = false;
            for (var index = 0; index < excelRows.Count; index++)
            {
                var excelRow = excelRows[index];
                var rowNumber = index + 2; // add 2 to reach the first data row because the first row is a header, excel row numbers start with 1 not 0
                TEntity entityToUpdate = null;
                try
                {
                    if (ExcelRowIsBlank(excelRow))
                        continue;

                    if (isImportingMatchableEntities)
                    {
                        var xlsxIdColName = selectedDict[idPropertyName];
                        var idStringValue = excelRow[xlsxIdColName];
                        entityToUpdate = await GetMatchedDbObject(finder, idStringValue, trueIdType);

                        ValidateDbResult(entityToUpdate, saveBehavior.RecordMode, xlsxIdColName, idStringValue);
                    }

                    if (entityToUpdate == null)
                    {
                        if (selectedDict.ContainsKey(pk.Name))
                        {
                            var trueIdXlsxIdFieldName = selectedDict[pk.Name];
                            EnsureNoEntityCreationWithIdWhenAutoIncrementIdType(pk.Name, isAutoIncrementingId, excelRow[trueIdXlsxIdFieldName]);
                        }

                        entityToUpdate = new TEntity();
                        _dbContext.Set<TEntity>().Add(entityToUpdate);
                    }

                    await MapIntoEntity(selectedDict, pk.Name, overridingMapper, entityToUpdate, excelRow, isAutoIncrementingId, saveBehavior.RecordMode);
                    if (validator != null)
                    {
                        var errors = validator.GetValidationErrors(entityToUpdate);

                        if(errors.Any())
                            throw new RowInvalidException(errors);
                    }
                    else
                    {
                        importResult.SuccessCount++;
                    }
                }
                catch (RowParseException e)
                {
                    HandleError(importResult.RowErrorDetails, rowNumber, entityToUpdate, "Error: " + e.Message);
                    foundErrors = true;
                }
                catch (RowInvalidException e)
                {
                    HandleError(importResult.RowErrorDetails, rowNumber, entityToUpdate, "Error: " + e.Message);
                    foundErrors = true;
                }
                catch (Exception)
                {
                    HandleError(importResult.RowErrorDetails, rowNumber, entityToUpdate, "Cannot be updated - error importing");
                    foundErrors = true;
                }

                if (saveBehavior.CommitMode == CommitMode.AnySuccessfulOneAtATime)
                {
                   await  _dbContext.SaveChangesAsync();
                } 
            }

            if ((saveBehavior.CommitMode == CommitMode.AnySuccessfulAtEndAsBulk) ||
                (saveBehavior.CommitMode == CommitMode.CommitAllAtEndIfAllGoodOrRejectAll && !foundErrors))
            {
                await _dbContext.SaveChangesAsync();
            }

            return importResult;
        }

        private void ValidateIdTypes<TEntity, TId>(string idPropertyName, EdmMember pk) where TEntity : class, new()
        {
            if (idPropertyName != null)
            {
                var alternateCol = GetEntityProperty(typeof(TEntity), idPropertyName);
                var idType = alternateCol.GetType();
                if (idType != typeof(TId))
                    throw new Exception("If using Surrogate ID, TId Type must be type of Surrogate ID");
            }
        }

        private async Task<List<Dictionary<string, string>>> GetExcelRows(DataMatchesForImport matchingData, string fileLocation)
        {
            if (matchingData.FileStream != null)
            {
                var streamFileFormat = matchingData.FileFormat ?? FileFormat.OpenExcel;
                return await _excelIoWrapper.GetRows(matchingData.FileStream, matchingData.Sheet, streamFileFormat);
            }

            var filePath = Path.Combine((fileLocation ?? Path.GetTempPath()), matchingData.FileName);
            var fileFormat = matchingData.FileFormat ?? GetFileFormatFromFile(matchingData.FileName);

            return await _excelIoWrapper.GetRows(filePath, matchingData.Sheet, fileFormat);
        }

        private FileFormat GetFileFormatFromFile(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            if (fileExtension == ".csv")
                return FileFormat.Csv;
            else if (fileExtension == ".xlsx")
            {
                return FileFormat.OpenExcel;
            }

            throw new NotSupportedException("XlsToEf only supports xlsx and csv files");
        }

        private Dictionary<string, string> BuildDictionaryFromSelected(List<XlsToEfColumnPair> selected)
        {
            var hasDuplicates = selected.GroupBy(x => x.EfName)
                .Select(group => new {Name = group.Key, Count = group.Count()})
                .Any(x => x.Count > 1);
            if (hasDuplicates)
            {
                throw new Exception("Destination targets must be unique");
            }
            var dict = selected.ToDictionary(x => x.EfName, x => x.XlsName);
            return dict;

        }

        private void HandleError<TEntity>(IDictionary<string, string> rowErrorDetails, int rowNumber, TEntity entityToRollBack, string message)
            where TEntity : class, new()
        {
            rowErrorDetails.Add(rowNumber.ToString(), message);
            if (entityToRollBack != null)
            {
                MarkForNotSaving(entityToRollBack);
            }
        }

        private void MarkForNotSaving<TEntity>(TEntity entityToUpdate) where TEntity : class, new()
        {
            if (_dbContext.Entry(entityToUpdate).State == EntityState.Added)
            {
                _dbContext.Entry(entityToUpdate).State = EntityState.Detached;
            }
            else
            {
                _dbContext.Entry(entityToUpdate).State = EntityState.Unchanged;
            }
        }

        private static async Task MapIntoEntity<TEntity>(Dictionary<string, string> matchingData, string idPropertyName,
            IUpdatePropertyOverrider<TEntity> overridingMapper, TEntity entityToUpdate,
            Dictionary<string, string> excelRow, bool isAutoIncrementingId, RecordMode recordMode)

        {
            IList<string> alreadyHandledPropertyNames = new List<string>();
            if (overridingMapper != null)
            {
                alreadyHandledPropertyNames =
                    await overridingMapper.UpdateProperties(entityToUpdate, matchingData, excelRow, recordMode);
            }
            UpdateProperties(entityToUpdate, matchingData, excelRow, idPropertyName, isAutoIncrementingId, alreadyHandledPropertyNames);

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

        private async Task<TEntity> GetMatchedDbObject<TEntity, TId>(Func<TId, Expression<Func<TEntity, bool>>> finder, string idStringValue, Type idType) where TEntity : class
        {
            if (string.IsNullOrWhiteSpace(idStringValue)) return null;

            TEntity matchedDbObject;

            if (finder != null)
            {
                var finderInputType = (TId) Convert.ChangeType(idStringValue,typeof(TId));
                var getExp = finder(finderInputType);
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

        private static void EnsureNoIdColumnIncludedWhenCreatingAutoIncrementEntities(RecordMode recordMode,
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

        private EdmProperty GetEntityProperty(Type eType, string propName)
        {
            var metadata = ((IObjectContextAdapter)_dbContext).ObjectContext.MetadataWorkspace;

            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));
            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace).Single(e => objectItemCollection.GetClrType(e) == eType);
            var prop = entityType.Properties.FirstOrDefault(x => x.Name == propName);
            return prop;
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
            Dictionary<string, string> excelRow, string selectorColName, bool shouldSkipIdInsert, IList<string> alreadyHandledPropertyNames) 
        {
            foreach (var entityPropertyName in matches.Keys)
            {
                if (alreadyHandledPropertyNames.Contains(entityPropertyName)) continue;
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

    public static class StringToTypeConverter
    {
        public static object Convert(string xlsxItemData, Type propertyType)
        {
            if (propertyType == typeof (string))
            {
                return xlsxItemData;
            }

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
            if (propertyType == typeof(Guid))
            {
                return new Guid(xlsxItemData);
            }

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
}