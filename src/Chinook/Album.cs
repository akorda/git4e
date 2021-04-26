using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace Chinook
{
    public class Album : HashableObject
    {
        /// <summary>
        /// The content type name of the <see cref="Chinook.Album"/>.
        /// </summary>
        public const string AlbumContentTypeName = "Album";

        [ProtoContract]
        class AlbumContent : IContent
        {
            [ProtoMember(1)]
            public int AlbumId { get; set; }
            [ProtoMember(2)]
            public string Title { get; set; }
            [ProtoMember(3)]
            public string TracksHash { get; set; }
            [ProtoMember(4)]
            public string[] TrackFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var tracks =
                    this.TrackFullHashes?
                    .Select(trackFullHash => new LazyTrack(repository, trackFullHash))
                    ?? new LazyTrack[0];
                var album = new Album(repository, hash)
                {
                    AlbumId = this.AlbumId,
                    Title = this.Title,
                    Tracks = new HashableList<LazyTrack>(repository, tracks, this.TracksHash)
                };
                return Task.FromResult(album as IHashableObject);
            }
        }

        int _AlbumId;

        [ContentProperty]
        public int AlbumId
        {
            get => _AlbumId;
            set
            {
                if (_AlbumId != value)
                {
                    _AlbumId = value;
                    this.MarkAsDirty();
                }
            }
        }

        string _Title;

        [ContentProperty]
        public string Title
        {
            get => _Title;
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    this.MarkAsDirty();
                }
            }
        }

        HashableList<LazyTrack> _Tracks;

        [ContentCollection]
        public HashableList<LazyTrack> Tracks
        {
            get => _Tracks;
            set
            {
                if (_Tracks != value)
                {
                    _Tracks = value;
                    this.MarkAsDirty();
                }
            }
        }

        //in memory only
        internal int ArtistId { get; set; }

        public Album()
            : this(null)
        {
        }

        public Album(IRepository repository, string hash = null)
            : base(repository, AlbumContentTypeName, hash)
        {
        }

        public static Type GetContentType() => typeof(AlbumContent);

        protected override IContent GetContent()
        {
            if (this.Tracks == null)
                this.Tracks = new HashableList<LazyTrack>(this.Repository);

            var content = new AlbumContent
            {
                AlbumId = this.AlbumId,
                Title = this.Title,
                TracksHash = this.Tracks.Hash,
                TrackFullHashes = this.Tracks.FullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var tracks = this.Tracks ?? new HashableList<LazyTrack>(this.Repository);
            foreach (var track in tracks)
            {
                yield return await Task.FromResult(track);
            }
        }

        public override string UniqueId => this.AlbumId.ToString();
    }

    /// <summary>
    /// Album Hash with no included properties
    /// </summary>
    public class LazyAlbum : LazyHashableObject
    {
        public LazyAlbum(IRepository repository, string fullHash)
            : base(repository, fullHash, Album.AlbumContentTypeName)
        {
        }

        public LazyAlbum(Album album)
            : base(album)
        {
        }

        public new Album LoadValue() => base.LoadValue<Album>();

        private int? _AlbumId;
        public int AlbumId
        {
            get
            {
                if (!_AlbumId.HasValue)
                    _AlbumId = int.Parse(this.UniqueId);
                return _AlbumId.Value;
            }
        }
    }
}
