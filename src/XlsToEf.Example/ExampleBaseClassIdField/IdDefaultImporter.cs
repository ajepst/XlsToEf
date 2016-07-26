using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleBaseClassIdField
{

    public class IdDefaultImporter
    {
        private readonly XlsxToTableImporter _importer;

        public IdDefaultImporter(XlsxToTableImporter importer)
        {
            _importer = importer;
        }

        public Task<ImportResult> ImportColumnData<TEntity, TSelector>(ImportMatchingData matchingData,
            UpdatePropertyOverrider<TEntity> overrider = null) where TEntity : Entity<TSelector>, new() where TSelector : IEquatable<TSelector>
        {
            Func<TSelector, Expression<Func<TEntity, bool>>> finderExpression =
                selectorValue => entity => entity.Id.Equals(selectorValue);
            return _importer.ImportColumnData(matchingData, finderExpression, "Id", overrider: overrider);
        }
    }
}
