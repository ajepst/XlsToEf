using System.Collections.Generic;

namespace XlsToEf.Import
{
    public interface IEntityValidator<T>
    {
        Dictionary<string, string> GetValidationErrors(T entity);
    }
}