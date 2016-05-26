using System;

namespace XlsToEf.Example.Domain
{
    public class Order : EntityBase<short>
    {
        public DateTime OrderDate { get; set; }
    }
}