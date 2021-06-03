using System;

namespace XlsToEf.Import
{
    public class RowParseException : Exception

    {
        public RowParseException(string message) : base(message)
        {
        }
    }
}