using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QService.Entities
{
    [DataContract]
    public class Level1
    {
        [DataMember]
        public decimal BestBidPrice { get; set; }

        [DataMember]
        public decimal BestBidVolume { get; set; }

        [DataMember]
        public decimal BestAskPrice { get; set; }

        [DataMember]
        public decimal BestAskVolume { get; set; }

        [IgnoreDataMember]
        public Security security { get; set; }
    }
}
