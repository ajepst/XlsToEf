namespace XlsToEf.Tests.Models
{
    public abstract class Entity<T> : BaseEntity
    {
        public int Id { get; set; }
    }
}