using System;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class Seaman : HashableObject
    {
        public const string SeamanContentType = "Seaman";

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

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var seaman = new Seaman(hash)
                {
                    SeamanCode = this.SeamanCode,
                    LastName = this.LastName,
                    FirstName = this.FirstName
                };
                return Task.FromResult(seaman as IHashableObject);
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

        public Seaman(string hash = null)
            : base(SeamanContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new SeamanContent
            {
                SeamanCode = this.SeamanCode,
                LastName = this.LastName,
                FirstName = this.FirstName
            };
            return content;
        }
    }
}
