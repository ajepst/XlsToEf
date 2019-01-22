using System.Collections.Generic;

namespace XlsToEf.Core.Import
{
    public class SheetPickerInformation
    {
        public IList<string> Sheets { get; set; }
        public string File { get; set; }
        public IList<UploadDestinationInformation> Destinations { get; set; }
    }

    public class UploadDestinationInformation
    {
        public string Name { get; set; }
        public string SelectSheetUrl { get; set; }
        public string MatchSubmitUrl { get; set; }
    }
}