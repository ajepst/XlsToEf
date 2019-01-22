using System;

namespace XlsToEf.Core.Tests.Models
{
    public class Order : Entity<int>
    {
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }
}