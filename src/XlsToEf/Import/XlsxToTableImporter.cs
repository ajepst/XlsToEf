using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public async Task<ImportResult> ImportColumnData<TEntity, TSelector>(ImportMatchingData matchingData, string selectorColName, Func<TSelector, Expression<Func<TEntity, bool>>> finder, UpdatePropertyOverrider<TEntity> overrider = null)
           where TEntity : class
        {
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

                    var xlsxSelectorColName = matchingData.Selected[selectorColName];
                    var selectorData = (TSelector) Convert.ChangeType(excelRow[xlsxSelectorColName], typeof(TSelector));

                    var getExp = finder(selectorData);

                    var matchedDbObject = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(getExp);
                    if (matchedDbObject == null)
                    {
                        importResult.RowErrorDetails.Add(rowNumber.ToString(), xlsxSelectorColName + " value " + selectorData +
                                                         " cannot be updated - not found in database");
                        continue;
                    }
                    if (overrider != null)
                    {
                        await overrider.UpdateProperties(matchedDbObject, matchingData.Selected, excelRow);
                    }
                    else
                    {
                        UpdateProperties(matchedDbObject, matchingData.Selected, excelRow, selectorColName);
                    }
                    importResult.SuccessCount++;
                }
                catch (RowParseException e)
                {
                    importResult.RowErrorDetails.Add(rowNumber.ToString(), "Error: " + e.Message);
                }
                catch
                {
                    importResult.RowErrorDetails.Add(rowNumber.ToString(), "Cannot be updated - error importing");
                }
            }
            _dbContext.SaveChanges();
            
            return importResult;
        }

        private static void UpdateProperties<TSelector>(TSelector matchedObject, Dictionary<string, string> matches,
            Dictionary<string, string> excelRow, string selectorColName) where TSelector : class
        {
            foreach (var tablePropertyName in matches.Keys)
            {
                if (tablePropertyName == selectorColName) continue;
                var xlsxColumnName = matches[tablePropertyName];
                var xlsxItemData = excelRow[xlsxColumnName];


                Type matchedObjectType = matchedObject.GetType();
                PropertyInfo propToSet = matchedObjectType.GetProperty(tablePropertyName);

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
}