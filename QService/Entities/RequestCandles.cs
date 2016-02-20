using System;

namespace QService.Entities
{
    public class RequestCandles
    {
        public StockSharp.BusinessEntities.Security Security { get; set; }
        public Type Type { get; set; }
        public TimeSpan TimeFrame { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}
