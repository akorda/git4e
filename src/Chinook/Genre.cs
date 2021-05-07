using System;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace Chinook
{
    public class Genre : HashableObject
    {
        public const string GenreContentType = "Genre";

        [ProtoContract]
        public class GenreContent : IContent
        {
            [ProtoMember(1)]
            public string GenreId { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var genre = new Genre(hash)
                {
                    GenreId = this.GenreId,
                    Name = this.Name
                };
                return Task.FromResult(genre as IHashableObject);
            }
        }

        string _GenreId;
        public string GenreId
        {
            get => _GenreId;
            set
            {
                if (_GenreId != value)
                {
                    _GenreId = value;
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

        public Genre()
            : this(null)
        {
        }

        public Genre(string hash = null)
            : base(GenreContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new GenreContent
            {
                GenreId = this.GenreId,
                Name = this.Name
            };
            return content;
        }
    }
}
