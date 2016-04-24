using StockSharp.BusinessEntities;
using StockSharp.Messages;
using System;
using System.Runtime.Serialization;

namespace QService.Entities
{
    [DataContract]
    public class Level2
    {
        //
        // Сводка:
        //      Возвращает массив офферов упорядоченных по возрастанию цены.Первый оффер будет иметь минимальную цену, и будет являться лучшим.
        [DataMember]
        public decimal[] Asks { get; }
        //
        // Сводка:
        //     Лучший оффер. Если стакан не содержит офферов, то будет возвращено null.
        [DataMember]
        public decimal BestAsk { get; }
        //
        // Сводка:
        //     Лучший бид. Если стакан не содержит бидов, то будет возвращено null.
        [DataMember]
        public decimal BestBid { get; }
        //
        // Сводка:
        //     Лучшая пара котировок. Если стакан пустой, то будет возвращено null.
        [DataMember]
        public MarketDepthPair BestPair { get; }
        //
        // Сводка:
        //     Возвращает массив бидов упорядоченных по убыванию цены. Первый бид будет иметь максимальную цену, и будет являться лучшим.
        [DataMember]
        public Quote[] Bids { get; }
        //
        // Сводка:
        //     	Общее количество котировок(бидов + оферов) в стакане.
        [DataMember]
        public int Count { get; }
        //
        // Сводка:
        //     Валюта торгового инструмента.
        [DataMember]
        public CurrencyTypes? Currency { get; set; }
        //
        // Сводка:
        //     Глубина стакана.
        [DataMember]
        public int Depth { get; }
        //
        // Сводка:
        //     Время последнего изменения стакана.
        [DataMember]
        public DateTimeOffset LastChangeTime { get; set; }
        //
        // Сводка:
        //     Локальное время последнего изменения стакана.
        [DataMember]
        public DateTime LocalTime { get; set; }
        //
        // Сводка:
        //     Максимальная глубина стакана.
        //
        [DataMember]
        public int MaxDepth { get; set; }
        //
        // Сводка:
        //     Security.
        [DataMember]
        public Security Security { get; }
        //
        // Сводка:
        //     Получить общий ценовой размер по офферам.
        [DataMember]
        public decimal TotalAsksPrice { get; }
        //
        // Сводка:
        //     Получить общий объем по офферам.
        [DataMember]
        public decimal TotalAsksVolume { get; }
        //
        // Сводка:
        //     Получить общий ценовой размер по бидам.
        [DataMember]
        public decimal TotalBidsPrice { get; }
        //
        // Сводка:
        //     	Получить общий объем по бидам.
        [DataMember]
        public decimal TotalBidsVolume { get; }
        //
        // Сводка:
        //     Получить общий ценовой размер.
        [DataMember]
        public decimal TotalPrice { get; }
        //
        // Сводка:
        //     Получить общий объем.
        [DataMember]
        public decimal TotalVolume { get; }

        public static explicit operator Level2(MarketDepth v)
        {
            if (v != null) {
                return new Level2
                {
                    Currency = v.Currency,
                    LastChangeTime = v.LastChangeTime,
                    LocalTime = v.LocalTime,
                    MaxDepth = v.MaxDepth    
                };
            }

            return null;
        }
    }
}
