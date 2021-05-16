using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace Chinook
{
    public class MediaType : HashableObject
    {
        public const string MediaTypeContentType = "MediaType";

        [ProtoContract]
        public class MediaTypeContent : IContent
        {
            [ProtoMember(1)]
            public string MediaTypeId { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var mediaType = new MediaType(repository, hash)
                {
                    MediaTypeId = this.MediaTypeId,
                    Name = this.Name
                };
                return Task.FromResult(mediaType as IHashableObject);
            }
        }

        string _MediaTypeId;
        public string MediaTypeId
        {
            get => _MediaTypeId;
            set
            {
                if (_MediaTypeId != value)
                {
                    _MediaTypeId = value;
                    this.MarkAsDirty();
                }
            }
        }
        string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    this.MarkAsDirty();
                }
            }
        }

        public MediaType()
            : this(null)
        {
        }

        public MediaType(IRepository repository, string hash = null)
            : base(repository, MediaTypeContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new MediaTypeContent
            {
                MediaTypeId = this.MediaTypeId,
                Name = this.Name
            };
            return content;
        }

        public override string UniqueId => this.MediaTypeId.ToString();
    }
}
