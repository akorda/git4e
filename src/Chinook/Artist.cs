using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace Chinook
{
    public class Artist : HashableObject
    {
        public const string ArtistContentType = "Artist";

        [ProtoContract]
        public class ArtistContent : IContent
        {
            [ProtoMember(1)]
            public int ArtistId { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string AlbumsHash { get; set; }
            [ProtoMember(4)]
            public string[] AlbumFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var albums =
                    this.AlbumFullHashes?
                    .Select(albumFullHash => new LazyAlbum(repository, albumFullHash))
                    ?? new LazyAlbum[0];
                var artist = new Artist(repository, hash)
                {
                    ArtistId = this.ArtistId,
                    Name = this.Name,
                    Albums = new HashableList<LazyAlbum>(repository, albums, this.AlbumsHash)
                };
                return Task.FromResult(artist as IHashableObject);
            }
        }

        int _ArtistId;
        public int ArtistId
        {
            get => _ArtistId;
            set
            {
                if (_ArtistId != value)
                {
                    _ArtistId = value;
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

        HashableList<LazyAlbum> _Albums;
        public HashableList<LazyAlbum> Albums
        {
            get => _Albums;
            set
            {
                if (_Albums != value)
                {
                    _Albums = value;
                    this.MarkAsDirty();
                }
            }
        }

        public Artist()
            : this(null)
        {
        }

        public Artist(IRepository repository, string hash = null)
            : base(repository, ArtistContentType, hash)
        {
        }

        protected override object GetContent()
        {
            if (this.Albums == null)
                this.Albums = new HashableList<LazyAlbum>(this.Repository);

            var content = new ArtistContent
            {
                ArtistId = this.ArtistId,
                Name = this.Name,
                AlbumsHash = this.Albums.Hash,
                AlbumFullHashes = this.Albums.FullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var albums = this.Albums ?? new HashableList<LazyAlbum>(this.Repository);
            foreach (var album in albums)
            {
                yield return await Task.FromResult(album);
            }
        }

        public override string UniqueId => this.ArtistId.ToString();
    }

    /// <summary>
    /// Artist Hash with the following included properties:
    /// 1. ArtistId
    /// 2. Artist Name
    /// </summary>
    public class LazyArtist : LazyHashableObject
    {
        public LazyArtist(IRepository repository, string fullHash)
            : base(repository, fullHash, Artist.ArtistContentType)
        {
        }

        public LazyArtist(Artist artist)
            : base(artist, artist.Name)
        {
        }

        public new Artist GetValue() => base.GetValue<Artist>();

        private int? _ArtistId;
        public int ArtistId
        {
            get
            {
                if (!_ArtistId.HasValue)
                    _ArtistId = int.Parse(this.UniqueId);
                return _ArtistId.Value;
            }
        }

        public string Name => this.IncludedProperties[0];
    }
}
