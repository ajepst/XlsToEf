using System;

namespace XlsToEf.Tests.Models
{
    public class Order : Entity<short>
    {
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }
}