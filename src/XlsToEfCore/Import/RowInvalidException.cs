using System;
using System.Collections.Generic;
using System.Linq;

namespace XlsToEfCore.Import
{
    public class RowInvalidException : Exception

    {
        public RowInvalidException(Dictionary<string, string> details) : base(MakeString(details))
        {
        }

        private static string MakeString(Dictionary<string, string> details)
        {
            return string.Join("\n", details.Select(kv => kv.Key + ": " + kv.Value));
        }
    }
}