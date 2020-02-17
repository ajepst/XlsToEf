using System.Collections.Generic;
using System.Threading.Tasks;

namespace XlsToEf.Core.Import
{
    public abstract class UpdatePropertyOverrider<TSelector>
    {
        public abstract Task UpdateProperties(TSelector destination, Dictionary<string, string> destinationProperty, Dictionary<string, string> value, RecordMode recordMode);
    }
}