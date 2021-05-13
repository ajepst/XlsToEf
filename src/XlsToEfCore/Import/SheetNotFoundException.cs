using System;

namespace XlsToEfCore.Import
{
    public class SheetNotFoundException : Exception
    {
        public SheetNotFoundException(string sheetName) : base(MakeString(sheetName))
        {
        }

        private static string MakeString(string sheetName)
        {
            return $"Sheet {sheetName} not found in spreadsheet";
        }
    }
}