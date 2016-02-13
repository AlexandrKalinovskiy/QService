using System.Runtime.Serialization;

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

        //[DataMember]
        //public virtual List<Security> Securities { get; set; }
    }
}
