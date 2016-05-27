using System;

namespace XlsToEf.Example.Domain
{
    public class Order : Entity<short>
    {
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }
}