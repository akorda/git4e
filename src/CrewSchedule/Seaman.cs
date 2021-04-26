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

            public IHashableObject ToHashableObject(IContentSerializer contentSerializer, IObjectLoader objectLoader, IHashCalculator hashCalculator)
            {
                return new Seaman(contentSerializer, hashCalculator)
                {
                    SeamanCode = this.SeamanCode,
                    LastName = this.LastName,
                    FirstName = this.FirstName
                };
            }
        }

        string _SeamanCode;
        public string SeamanCode
        {
            get => _SeamanCode;
            set
            {
                if (_SeamanCode != value)
                {
                    _SeamanCode = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        string _FirstName;
        public string FirstName
        {
            get => _FirstName;
            set
            {
                if (_FirstName != value)
                {
                    _FirstName = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        string _LastName;
        public string LastName
        {
            get => _LastName;
            set
            {
                if (_LastName != value)
                {
                    _LastName = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        //Compatibilities, etc

        public Seaman(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("Seaman", contentSerializer, hashCalculator)
        {
        }

        public override void SerializeContent(Stream stream)
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
