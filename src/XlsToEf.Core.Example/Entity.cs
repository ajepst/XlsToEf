namespace XlsToEfCore.Example
{
    public abstract class Entity<T> : BaseEntity
    {
        public T Id { get; set; }
    }
}