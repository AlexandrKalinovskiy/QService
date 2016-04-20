using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using StockSharp.BusinessEntities;

namespace QService.Entities
{
    [DataContract]
    public class News
    {
        [DataMember]
        public ExchangeBoard Board { get; set; }
        [DataMember]
        public IDictionary<object, object> ExtensionInfo { get; set; }
        [DataMember]
        public string Headline { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public DateTime LocalTime { get; set; }
        [DataMember]
        public Security Security { get; set; }
        [DataMember]
        public DateTimeOffset ServerTime { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public string Story { get; set; }
        [DataMember]
        public Uri Url { get; set; }

        public static explicit operator News(StockSharp.BusinessEntities.News v)
        {
            return new News
            {
                Board = (ExchangeBoard)v.Board,
                Security = (Security)v.Security,
                Headline = v.Headline,
                Id = v.Id,
                LocalTime = v.LocalTime,
                ExtensionInfo = v.ExtensionInfo,
                ServerTime = v.ServerTime,
                Source = v.Source,
                Story = v.Story,
                Url = v.Url
            };
        }
    }
}
