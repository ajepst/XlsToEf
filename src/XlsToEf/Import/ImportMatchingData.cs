using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;

namespace XlsToEf.Import
{
    public class ImportMatchingData
    {
        public string FileName { get; set; }
        public string Sheet { get; set; }
        public List<XlsToEfColumnPair> Selected { get; set; } 
    }

    public class XlsToEfColumnPair
    {
        public string XlsName { get; set; }

        public string EfName { get; set; }

        public XlsToEfColumnPair() 
        {
        }

        private XlsToEfColumnPair(string xlsName, string efName)
        {
            XlsName = xlsName;
            EfName = efName;
        }

        public static XlsToEfColumnPair Create(string xlsName, string efName)
        {
            return new XlsToEfColumnPair(xlsName, efName);
        }

        public static XlsToEfColumnPair Create<T>(string xlsName, Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            var name = me.Member.Name;
            return new XlsToEfColumnPair(xlsName, name);
        }
    }
}