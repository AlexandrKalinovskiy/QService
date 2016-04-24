using System.Runtime.Serialization;

namespace QService.Entities
{
    [DataContract]
    public enum MarketDataTypes
    {
        [EnumMember]
        Level1 = 0,

        [EnumMember]
        Level2 = 1,

        //[EnumMember]
        //Trades = 2,

        //[EnumMember]
        //OrderLog = 3,

        [EnumMember]
        News = 4,

        //[EnumMember]
        //CandleTimeFrame = 5,

        //[EnumMember]
        //CandleTick = 6,

        //[EnumMember]
        //CandleVolume = 7,

        //[EnumMember]
        //CandleRange = 8,

        //[EnumMember]
        //CandlePnF = 9,

        //[EnumMember]
        //CandleRenko = 10
    }
}
