using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XlsToEf.Import
{
    public class ImportColumnData
    {
        public ImportColumnData()
        {
            RequiredTogether = new string[0][];
        }

        public string[] XlsxColumns { get; set; }
        //public Dictionary<string, SingleColumnData> TableColumns { get; set; }
        public List<TableColumnConfiguration> TableColumns { get; set; }
        public string FileName { get; set; }
        public string[][] RequiredTogether { get; set; }
    }

    public class SingleColumnData
    {
        public SingleColumnData(string name, bool required = true)
        {
            Name = name;
            Required = required;
        }

        public string Name { get; private set; }
        public bool Required { get; private set; }
    }

    public class TableColumnConfiguration
    {
        public string Name { get; }

        public SingleColumnData ColumnData { get; }

        private TableColumnConfiguration(string columnName, SingleColumnData columnData)
        {
            Name = columnName;
            ColumnData = columnData;
        }

        public static TableColumnConfiguration Create<T>(Expression<Func<T>> propertyLambda,  SingleColumnData columnData)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            var name =  me.Member.Name;

            var obj = new TableColumnConfiguration(name, columnData);
            return obj;

        }

        public static TableColumnConfiguration Create(string name, SingleColumnData columnData)
        {
            var obj = new TableColumnConfiguration(name, columnData);
            return obj;

        }
    }
}