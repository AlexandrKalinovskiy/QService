using System.Runtime.Serialization;

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
    }
}
