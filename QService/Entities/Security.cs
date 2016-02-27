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
            if (s != null)
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

            return null;
        }

        public static explicit operator Security(StockSharp.BusinessEntities.Security s)
        {
            if (s != null)
            {
                return new Security
                {
                    Ticker = s.Code,
                    Code = s.Id,
                    Name = s.Name,
                    ExchangeBoard = new ExchangeBoard
                    {
                        Code = s.Board.Code
                    }
                };
            }

            return null;
        }
    }
}
