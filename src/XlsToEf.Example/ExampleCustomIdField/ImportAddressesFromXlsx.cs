using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MediatR;
using XlsToEf.Example.Domain;
using XlsToEf.Import;

namespace XlsToEf.Example.ExampleCustomIdField
{
    public class ImportAddressesFromXlsx : IAsyncRequestHandler<ImportMatchingAddressData, ImportResult>
    {
        private readonly XlsxToTableImporter _xlsxToTableImporter;

        public ImportAddressesFromXlsx(XlsxToTableImporter xlsxToTableImporter)
        {
            _xlsxToTableImporter = xlsxToTableImporter;
        }

        public async Task<ImportResult> Handle(ImportMatchingAddressData message)
        {
            Func<string, Expression<Func<Address, bool>>> finderExpression = selectorValue => address => address.AddrId == selectorValue;
            var selectorColName = GetSelectorColName();
            return await _xlsxToTableImporter.ImportColumnData(message, selectorColName, finderExpression);
        }

        private static string GetSelectorColName()
        {
            var wo = new Address();
            var selectorColName = ImportColumnDataBuilder.GetPropertyName(() => wo.AddrId);
            return selectorColName;
        }
    }
}