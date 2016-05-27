namespace XlsToEf.Example
{
    public abstract class Entity<T> : BaseEntity
    {
        public int Id { get; set; }
    }
}