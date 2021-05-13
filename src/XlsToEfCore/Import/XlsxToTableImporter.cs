using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace XlsToEfCore.Import
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
        public Task<ImportResult> ImportColumnData<TEntity>(DataMatchesForImport matchingData, ImportSaveBehavior saveBehavior = null, string fileLocation = null)
            where TEntity : class, new()
        {
            return ImportColumnData<TEntity, string>(matchingData, null, null, null, saveBehavior, fileLocation: fileLocation);
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
            var isDbGeneratedId = IsIdDbGenerated(typeof(TEntity));

            var keyInfo = GetEntityKeys(typeof(TEntity));
            EnsureImportingEntityHasSingleKey(keyInfo);
            var pk = keyInfo[0];

            ValidateIdTypes<TEntity, TId>(idPropertyName, pk);

            var isImportingEntityId = selectedDict.ContainsKey(pk.Name);
            EnsureNoIdColumnIncludedWhenCreatingAutoIncrementEntities(saveBehavior.RecordMode, isDbGeneratedId,
                isImportingEntityId);

            if (idPropertyName == null)
            {
                idPropertyName = pk.Name;
            }

            var isImportingMatchableEntities = selectedDict.ContainsKey(idPropertyName);

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
                        entityToUpdate = await GetMatchedDbObject(finder, idStringValue, pk.ClrType);

                        ValidateDbResult(entityToUpdate, saveBehavior.RecordMode, xlsxIdColName, idStringValue);
                    }

                    if (entityToUpdate == null)
                    {
                        if (selectedDict.ContainsKey(pk.Name))
                        {
                            var trueIdXlsxIdFieldName = selectedDict[pk.Name];
                            EnsureNoEntityCreationWithIdWhenAutoIncrementIdType(pk.Name, isDbGeneratedId, excelRow[trueIdXlsxIdFieldName]);
                        }
                        entityToUpdate = new TEntity();
                        await _dbContext.Set<TEntity>().AddAsync(entityToUpdate);
                    }

                    await MapIntoEntity(selectedDict, pk.Name, overridingMapper, entityToUpdate, excelRow, isDbGeneratedId, saveBehavior.RecordMode);
                    if (validator != null)
                    {
                        var errors = validator.GetValidationErrors(entityToUpdate);

                        if (errors.Any())
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
                catch (Exception e)
                {
                    HandleError(importResult.RowErrorDetails, rowNumber, entityToUpdate, "Cannot be updated - error importing");
                    foundErrors = true;
                }

                if (saveBehavior.CommitMode == CommitMode.AnySuccessfulOneAtATime)
                {
                    await _dbContext.SaveChangesAsync();
                }
            }

            if ((saveBehavior.CommitMode == CommitMode.AnySuccessfulAtEndAsBulk) ||
                (saveBehavior.CommitMode == CommitMode.CommitAllAtEndIfAllGoodOrRejectAll && !foundErrors))
            {
                await _dbContext.SaveChangesAsync();
            }

            return importResult;
        }


        private void ValidateIdTypes<TEntity, TId>(string idPropertyName, IProperty pk) where TEntity : class, new()
        {
            if (idPropertyName != null)
            {
                var alternateCol = GetEntityProperty(typeof(TEntity), idPropertyName);
                var idType = alternateCol.PropertyInfo.PropertyType;
                if (idType != typeof(TId))
                    throw new Exception("If using Surrogate ID, TId Type must be type of Surrogate ID");
            }
        }

        private Task<List<Dictionary<string, string>>> GetExcelRows(DataMatchesForImport matchingData, string fileLocation)
        {
            if(matchingData.FileStream != null)
                return _excelIoWrapper.GetRows(matchingData.FileStream, matchingData.Sheet);

            var filePath = Path.Combine((fileLocation ?? Path.GetTempPath()), matchingData.FileName);

            return _excelIoWrapper.GetRows(filePath, matchingData.Sheet);
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
            _dbContext.Entry(entityToUpdate).State = _dbContext.Entry(entityToUpdate).State == EntityState.Added
                ? EntityState.Detached
                : EntityState.Unchanged;
        }

        private async Task MapIntoEntity<TEntity>(Dictionary<string, string> matchingData, string trueIdPropName,
            IUpdatePropertyOverrider<TEntity> overridingMapper, TEntity entityToUpdate, Dictionary<string, string> excelRow, bool isAutoIncrementingId, RecordMode recordMode) where TEntity : class

        {
            IList<string> alreadyHandledPropertyNames = new List<string>();
            if (overridingMapper != null)
            {
                alreadyHandledPropertyNames =  await overridingMapper.UpdateProperties(entityToUpdate, matchingData, excelRow, recordMode);
            }
            UpdateProperties(entityToUpdate, matchingData, excelRow, trueIdPropName, isAutoIncrementingId, alreadyHandledPropertyNames);
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

        private void EnsureImportingEntityHasSingleKey(IReadOnlyList<IProperty> keyInfo) { 


            if (keyInfo.Count > 1)
            {
                throw new Exception("XlsToEf only supports Single Column Key right now");
            }
        }

        private bool IsIdDbGenerated(Type eType)
        {
            var key = GetMappedKeyInformation(eType);
            var keyProperty = key.Properties[0];
            var idKeyAnnotation = keyProperty.FindAnnotation("SqlServer:ValueGenerationStrategy");
            var isGeneratedOnAdd = key.Properties[0]?.ValueGenerated == ValueGenerated.OnAdd;

            return ((idKeyAnnotation?.Value != null) && (idKeyAnnotation.Value.ToString().Contains("Identity") ||
                                               idKeyAnnotation.Value.ToString().Contains("Computed"))) ||
                   (isGeneratedOnAdd);

        }

        private IReadOnlyList<IProperty> GetEntityKeys(Type eType)
        {
            var keys = _dbContext.Model.FindEntityType(eType).FindPrimaryKey().Properties;
            return keys;
        }

        private IProperty GetEntityProperty(Type eType, string propName)
        {
            var prop = _dbContext.Model.FindEntityType(eType).FindProperty(propName);
            return prop;
        }

        private IKey GetMappedKeyInformation(Type eType)
        {
            return _dbContext.Model.FindEntityType(eType).FindPrimaryKey();
        }

        private void UpdateProperties<TSelector>(TSelector matchedObject, Dictionary<string, string> matches,
            Dictionary<string, string> excelRow, string trueIdPropName, bool shouldSkipIdInsert, IList<string> alreadyHandledPropertyNames) where TSelector : class
        {
            foreach (var entityPropertyName in matches.Keys)
            {
                if (alreadyHandledPropertyNames.Contains(entityPropertyName))
                    continue;
                if ((entityPropertyName == trueIdPropName) && shouldSkipIdInsert) continue;
                var xlsxColumnName = matches[entityPropertyName];
                var xlsxItemData = excelRow[xlsxColumnName];

                Type matchedObjectType = matchedObject.GetType();
                PropertyInfo propToSet = matchedObjectType.GetProperty(entityPropertyName);
                if (propToSet is null)
                {
                    var shadow = _dbContext.Entry(matchedObject).Property(entityPropertyName);
                    var shadowType = shadow.Metadata.ClrType;
                    var converted = StringToTypeConverter.Convert(xlsxItemData,shadowType);
                    shadow.CurrentValue = converted;
                }
                else
                {
                    var converted = StringToTypeConverter.Convert(xlsxItemData, propToSet.PropertyType);

                    propToSet.SetValue(matchedObject, converted, null);
                }
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