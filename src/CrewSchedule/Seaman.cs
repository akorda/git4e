using System.IO;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class Seaman : HashableObject
    {
        [ProtoContract]
        public class SeamanContent : IContent
        {
            [ProtoMember(1)]
            public string SeamanCode { get; set; }
            [ProtoMember(2)]
            public string LastName { get; set; }
            [ProtoMember(3)]
            public string FirstName { get; set; }
            //Compatibilities, etc

            public object ToObject(IContentSerializer contentSerializer, IObjectLoader objectLoader)
            {
                return new Seaman(contentSerializer)
                {
                    SeamanCode = this.SeamanCode,
                    LastName = this.LastName,
                    FirstName = this.FirstName
                };
            }
        }

        public string SeamanCode { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        //Compatibilities, etc

        public Seaman(IContentSerializer contentSerializer)
            : base("Seaman", contentSerializer)
        {
        }

        public override void SerializeContent(Stream stream, IHashCalculator hashCalculator)
        {
            var content = new SeamanContent
            {
                SeamanCode = this.SeamanCode,
                LastName = this.LastName,
                FirstName = this.FirstName
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }
    }
}
