using System.Collections.Generic;

namespace XlsToEf.Core.Import
{
    public interface IEntityValidator<T>
    {
        Dictionary<string, string> GetValidationErrors(T entity);
    }
}