using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XlsToEf.Import;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests.ImportHelperFiles
{
    public class IdDefaultWithCreateImporter
    {
        private readonly XlsxToTableImporter _importer;

        public IdDefaultWithCreateImporter(XlsxToTableImporter importer)
        {
            _importer = importer;
        }

        public Task<ImportResult> ImportColumnData<TEntity, TSelector>(ImportMatchingData matchingData,
            UpdatePropertyOverrider<TEntity> overrider = null) where TEntity : Entity<TSelector>, new() where TSelector : IEquatable<TSelector>
        {
            Func<TSelector, Expression<Func<TEntity, bool>>> finderExpression =
                selectorValue => entity => entity.Id.Equals(selectorValue);
            return _importer.ImportColumnData(matchingData, finderExpression, "Id", overrider: overrider, recordMode: RecordMode.Upsert);
        }
    }
}