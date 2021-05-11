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

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var seaman = new Seaman(repository, hash)
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
                    this.MarkAsDirty();
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
                    this.MarkAsDirty();
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
                    this.MarkAsDirty();
                }
            }
        }

        //Compatibilities, etc

        public Seaman(IRepository repository, string hash = null)
            : base(repository, SeamanContentType, hash)
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
