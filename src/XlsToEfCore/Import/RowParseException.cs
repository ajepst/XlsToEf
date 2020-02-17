using System;

namespace XlsToEf.Core.Import
{
    public class RowParseException : Exception

    {
        public RowParseException(string message) : base(message)
        {
        }
    }
}