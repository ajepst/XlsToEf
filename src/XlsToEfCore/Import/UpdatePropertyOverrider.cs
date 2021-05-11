using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XlsToEfCore.Import
{
    [Obsolete("Please use interface IUpdatePropertyOverrider")]
    public abstract class UpdatePropertyOverrider<TSelector>
    {
        public abstract Task UpdateProperties(TSelector destination, Dictionary<string, string> destinationProperty, Dictionary<string, string> value, RecordMode recordMode);
    }

    public interface IUpdatePropertyOverrider<in TSelector>
    {
        Task UpdateProperties(TSelector destination, Dictionary<string, string> destinationProperty, Dictionary<string, string> value, RecordMode recordMode);
    }
}