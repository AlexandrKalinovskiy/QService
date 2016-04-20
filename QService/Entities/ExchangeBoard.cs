using System;
using System.Runtime.Serialization;
using StockSharp.BusinessEntities;

namespace QService.Entities
{
    [DataContract]
    public class ExchangeBoard
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        public static explicit operator ExchangeBoard(StockSharp.BusinessEntities.ExchangeBoard v)
        {
            if (v != null)
            {
                return new ExchangeBoard
                {
                    Code = v.Code,
                    Name = v.Exchange.Name
                };
            }

            return new ExchangeBoard();
        }

        //[DataMember]
        //public virtual List<Security> Securities { get; set; }
    }
}
