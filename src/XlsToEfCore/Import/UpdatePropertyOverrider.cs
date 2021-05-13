using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XlsToEfCore.Import
{
    [Obsolete("Please use interface IUpdatePropertyOverrider", true)]
    public abstract class UpdatePropertyOverrider<TSelector>
    {
        public abstract Task UpdateProperties(TSelector destination, Dictionary<string, string> destinationProperty, Dictionary<string, string> value, RecordMode recordMode);
    }

    public interface IUpdatePropertyOverrider<in TEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity">The type of EF Entity</typeparam>
        /// <param name="destination">Destination entity to save data to</param>
        /// <param name="matches">Dictionary containing the entity destination property name as key, and the excel field name as the value</param>
        /// <param name="excelRow">dictionary containing all of the incoming excel data, key is excel field name</param>
        /// <param name="recordMode">value of RecordMode enumeration to allow implementation to obey recordMode that was set</param>
        /// <returns>Return list should be a collection of the entity destination property names
        /// for all properties that should be considered already handled by the overrider. The system will handle the
        /// remaining mapped properties </returns>
        Task<IList<string>> UpdateProperties(TEntity destination, Dictionary<string, string> matches, Dictionary<string, string> excelRow, RecordMode recordMode);
    }
}