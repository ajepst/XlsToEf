using System.ComponentModel.DataAnnotations.Schema;

namespace XlsToEfCore.Tests.Models
{
    [Table("ColorOptions")]
    public class ProductColorOption : Entity<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; }

        public string Color { get; set; }
    }

    [Table("SizeOptions")]
    public class ProductSizeOption : Entity<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; }

        public string Size { get; set; }

        public Product Product { get; set; }
    }
}