using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QService.Entities
{
    [DataContract]
    public enum Level1
    {
        [EnumMember]
        OpenPrice = 0,

        [EnumMember]
        HighPrice = 1,

        [EnumMember]
        LowPrice = 2,

        [EnumMember]
        ClosePrice = 3,

        [EnumMember]
        LastTrade = 4,

        [EnumMember]
        StepPrice = 5,

        [EnumMember]
        BestBid = 6,

        [EnumMember]
        BestAsk = 7,

        [EnumMember]
        ImpliedVolatility = 8,

        [EnumMember]
        TheorPrice = 9,

        [EnumMember]
        OpenInterest = 10,

        [EnumMember]
        MinPrice = 11,

        [EnumMember]
        MaxPrice = 12,

        [EnumMember]
        BidsVolume = 13,

        [EnumMember]
        BidsCount = 14,

        [EnumMember]
        AsksVolume = 15,

        [EnumMember]
        AsksCount = 16,

        [EnumMember]
        HistoricalVolatility = 17,

        [EnumMember]
        Delta = 18,

        [EnumMember]
        Gamma = 19,

        [EnumMember]
        Vega = 20,

        [EnumMember]
        Theta = 21,

        [EnumMember]
        MarginBuy = 22,

        [EnumMember]
        MarginSell = 23,

        [EnumMember]
        PriceStep = 24,

        [EnumMember]
        VolumeStep = 25,

        [EnumMember]
        ExtensionInfo = 26,

        [EnumMember]
        State = 27,

        [EnumMember]
        LastTradePrice = 28,

        [EnumMember]
        LastTradeVolume = 29,

        [EnumMember]
        Volume = 30,

        [EnumMember]
        AveragePrice = 31,

        [EnumMember]
        SettlementPrice = 32,

        [EnumMember]
        Change = 33,

        [EnumMember]
        BestBidPrice = 34,

        [EnumMember]
        BestBidVolume = 35,

        [EnumMember]
        BestAskPrice = 36,

        [EnumMember]
        BestAskVolume = 37,

        [EnumMember]
        Rho = 38,

        [EnumMember]
        AccruedCouponIncome = 39,

        [EnumMember]
        HighBidPrice = 40,

        [EnumMember]
        LowAskPrice = 41,

        [EnumMember]
        Yield = 42,

        [EnumMember]
        LastTradeTime = 43,

        [EnumMember]
        TradesCount = 44,

        [EnumMember]
        VWAP = 45,

        [EnumMember]
        LastTradeId = 46,

        [EnumMember]
        BestBidTime = 47,

        [EnumMember]
        BestAskTime = 48,

        [EnumMember]
        LastTradeUpDown = 49,

        [EnumMember]
        LastTradeOrigin = 50,

        [EnumMember]
        Multiplier = 51,

        [EnumMember]
        PriceEarnings = 52,

        [EnumMember]
        ForwardPriceEarnings = 53,

        [EnumMember]
        PriceEarningsGrowth = 54,

        [EnumMember]
        PriceSales = 55,

        [EnumMember]
        PriceBook = 56,

        [EnumMember]
        PriceCash = 57,

        [EnumMember]
        PriceFreeCash = 58,

        [EnumMember]
        Payout = 59,

        [EnumMember]
        SharesOutstanding = 60,

        [EnumMember]
        SharesFloat = 61,

        [EnumMember]
        FloatShort = 62,

        [EnumMember]
        ShortRatio = 63,

        [EnumMember]
        ReturnOnAssets = 64,

        [EnumMember]
        ReturnOnEquity = 65,

        [EnumMember]
        ReturnOnInvestment = 66,

        [EnumMember]
        CurrentRatio = 67,

        [EnumMember]
        QuickRatio = 68,

        [EnumMember]
        LongTermDebtEquity = 69,

        [EnumMember]
        TotalDebtEquity = 70,

        [EnumMember]
        GrossMargin = 71,

        [EnumMember]
        OperatingMargin = 72,

        [EnumMember]
        ProfitMargin = 73,

        [EnumMember]
        Beta = 74,

        [EnumMember]
        AverageTrueRange = 75,

        [EnumMember]
        HistoricalVolatilityWeek = 76,

        [EnumMember]
        HistoricalVolatilityMonth = 77,

        [EnumMember]
        IsSystem = 78,

        [EnumMember]
        Decimals = 79
    }
}
