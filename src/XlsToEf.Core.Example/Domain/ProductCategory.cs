namespace XlsToEf.Core.Example.Domain
{
    public class ProductCategory : Entity<int>
    {
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
    }
}