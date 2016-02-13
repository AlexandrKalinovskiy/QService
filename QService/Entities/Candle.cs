using System;

namespace QService.Entities
{
    public class Candle
    {
        public int Id { get; set; }
        public decimal OpenPrice { get; set; }
        public DateTimeOffset OpenTime { get; set; }    //Время в UTC
        public decimal HighPrice { get; set; }
        public DateTimeOffset HighTime { get; set; }    //Время в UTC
        public decimal LowPrice { get; set; }
        public DateTimeOffset LowTime { get; set; }     //Время в UTC
        public decimal ClosePrice { get; set; }
        public DateTimeOffset CloseTime { get; set; }   //Время в UTC
        public decimal TotalVolume { get; set; }
        public Security Security { get; set; }
    }
}
