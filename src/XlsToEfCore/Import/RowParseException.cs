using System;

namespace XlsToEfCore.Import
{
    public class RowParseException : Exception

    {
        public RowParseException(string message) : base(message)
        {
        }
    }
}