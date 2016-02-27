using System;
using System.Runtime.Serialization;
using StockSharp.BusinessEntities;

namespace QService.Entities
{
    [DataContract]
    public class Security
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Ticker { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public decimal? StepPrice { get; set; }

        [DataMember]
        public virtual ExchangeBoard ExchangeBoard { get; set; }

        public static explicit operator StockSharp.BusinessEntities.Security(Security s)
        {
            return new StockSharp.BusinessEntities.Security
            {
                Code = s.Ticker,
                Id = s.Code,
                Board = new StockSharp.BusinessEntities.ExchangeBoard
                {
                    Code = s.ExchangeBoard.Code
                }
            };
        }
    }
}
