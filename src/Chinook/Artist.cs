using System;
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
            public string[] AlbumFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var albums =
                    this.AlbumFullHashes
                    .Select(posHash => new LazyAlbum(repository, posHash))
                    .ToList();
                var artist = new Artist(repository, hash)
                {
                    ArtistId = this.ArtistId,
                    Name = this.Name,
                    Albums = albums
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

        List<LazyAlbum> _Albums;
        public List<LazyAlbum> Albums
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
            var albumFullHashes = this.Albums?
                .OrderBy(album => album.HashIncludeProperty1)
                .Select(album => album.FullHash)
                .ToArray();
            var content = new ArtistContent
            {
                ArtistId = this.ArtistId,
                Name = this.Name,
                AlbumFullHashes = albumFullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var albums = this.Albums ?? new List<LazyAlbum>();
            foreach (var album in albums)
            {
                yield return await Task.FromResult(album);
            }
        }
    }

    /// <summary>
    /// Artist Hash with the following included properties:
    /// 1. ArtistId
    /// 2. Artist Name
    /// </summary>
    public class LazyArtist : LazyHashableObject<int, string>
    {
        public LazyArtist(IRepository repository, string fullHash)
            : base(repository, fullHash, Artist.ArtistContentType)
        {
        }

        public LazyArtist(Artist artist)
            : base(artist, a => (a as Artist).ArtistId, a => (a as Artist).Name)
        {
        }
    }
}
