using System.Collections.Generic;

namespace XlsToEfCore.Import
{
    public interface IEntityValidator<T>
    {
        Dictionary<string, string> GetValidationErrors(T entity);
    }
}