using System;

namespace XlsToEfCore.Tests.Models
{
    public class Order : Entity<int>
    {
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }
}