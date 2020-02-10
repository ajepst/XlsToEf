using System;

namespace XlsToEf.Core.Example.Domain
{
    public class Order : Entity<int>
    {
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }
}