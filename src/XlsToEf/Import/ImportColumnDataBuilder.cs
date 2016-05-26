using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace XlsToEf.Import
{
    public class ImportColumnDataBuilder
    {
        private readonly IExcelIoWrapper _excelIoWrapper;

        public ImportColumnDataBuilder(IExcelIoWrapper excelIoWrapper)
        {
            _excelIoWrapper = excelIoWrapper;
        }

        public async Task<IList<string>> GetImportColumnData(XlsxColumnMatcherQuery matcherQuery)
        {
            return await _excelIoWrapper.GetColumns(matcherQuery.FilePath, matcherQuery.Sheet);
        }

        public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return me.Member.Name;
        }
    }
}