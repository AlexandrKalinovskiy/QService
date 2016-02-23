using System;
using System.Runtime.Serialization;

namespace QService.Entities
{
    [DataContract]
    public class Candle
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public decimal OpenPrice { get; set; }

        [DataMember]
        public DateTimeOffset OpenTime { get; set; }    //Время в UTC

        [DataMember]
        public decimal HighPrice { get; set; }

        [DataMember]
        public decimal LowPrice { get; set; }

        [DataMember]
        public decimal ClosePrice { get; set; }

        [DataMember]
        public DateTimeOffset CloseTime { get; set; }   //Время в UTC

        [DataMember]
        public decimal TotalVolume { get; set; }

        [DataMember]
        public Security Security { get; set; }
    }
}
