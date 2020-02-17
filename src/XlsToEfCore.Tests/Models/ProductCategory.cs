namespace XlsToEfCore.Tests.Models
{
    public class ProductCategory : Entity<int>
    {
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
    }
}