using System;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace Chinook
{
    public class Genre : HashableObject
    {
        /// <summary>
        /// The content type name of the <see cref="Chinook.Genre"/>.
        /// </summary>
        public const string GenreContentTypeName = "Genre";

        [ProtoContract]
        public class GenreContent : IContent
        {
            [ProtoMember(1)]
            public string GenreId { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var genre = new Genre(repository, hash)
                {
                    GenreId = this.GenreId,
                    Name = this.Name
                };
                return Task.FromResult(genre as IHashableObject);
            }
        }

        string _GenreId;

        [ContentProperty]
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

        [ContentProperty]
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

        public Genre(IRepository repository, string hash = null)
            : base(repository, GenreContentTypeName, hash)
        {
        }

        public static Type GetContentType() => typeof(GenreContent);

        protected override IContent GetContent()
        {
            var content = new GenreContent
            {
                GenreId = this.GenreId,
                Name = this.Name
            };
            return content;
        }

        public override string UniqueId => this.GenreId.ToString();
    }
}
