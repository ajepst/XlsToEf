namespace XlsToEf.Example.Domain
{
    public class Product : Entity<int>
    {
        public string ProductName { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public int ProductCategoryId { get; set; }
    }
}