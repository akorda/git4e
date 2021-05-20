using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace Chinook
{
    public class Track : HashableObject
    {
        public const string TrackContentType = "Track";

        [ProtoContract]
        public class TrackContent : IContent
        {
            [ProtoMember(1)]
            public int TrackId { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string Composer { get; set; }
            [ProtoMember(4)]
            public int Milliseconds { get; set; }
            [ProtoMember(5)]
            public int Bytes { get; set; }
            [ProtoMember(6)]
            public decimal UnitPrice { get; set; }
            [ProtoMember(7)]
            public string MediaTypeFullHash { get; set; }
            [ProtoMember(8)]
            public string GenreFullHash { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var track = new Track(repository, hash)
                {
                    TrackId = this.TrackId,
                    Name = this.Name,
                    Composer = this.Composer,
                    Milliseconds = this.Milliseconds,
                    Bytes = this.Bytes,
                    UnitPrice = this.UnitPrice,
                    MediaType = new LazyHashableObject(repository, this.MediaTypeFullHash, Chinook.MediaType.MediaTypeContentType),
                    Genre = new LazyHashableObject(repository, this.GenreFullHash, Chinook.Genre.GenreContentType)
                };
                return Task.FromResult(track as IHashableObject);
            }
        }

        int _TrackId;

        [ContentProperty]
        public int TrackId
        {
            get => _TrackId;
            set
            {
                if (_TrackId != value)
                {
                    _TrackId = value;
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

        string _Composer;

        [ContentProperty]
        public string Composer
        {
            get => _Composer;
            set
            {
                if (_Composer != value)
                {
                    _Composer = value;
                    this.MarkAsDirty();
                }
            }
        }

        int _Milliseconds;

        [ContentProperty]
        public int Milliseconds
        {
            get => _Milliseconds;
            set
            {
                if (_Milliseconds != value)
                {
                    _Milliseconds = value;
                    this.MarkAsDirty();
                }
            }
        }

        int _Bytes;

        [ContentProperty]
        public int Bytes
        {
            get => _Bytes;
            set
            {
                if (_Bytes != value)
                {
                    _Bytes = value;
                    this.MarkAsDirty();
                }
            }
        }

        decimal _UnitPrice;

        [ContentProperty]
        public decimal UnitPrice
        {
            get => _UnitPrice;
            set
            {
                if (_UnitPrice != value)
                {
                    _UnitPrice = value;
                    this.MarkAsDirty();
                }
            }
        }

        //in memory only
        internal int AlbumId { get; set; }

        //do not serialize
        internal string GenreId { get; set; }

        [ContentProperty]
        public LazyHashableObject Genre { get; set; }

        //do not serialize
        internal string MediaTypeId { get; set; }

        [ContentProperty]
        public LazyHashableObject MediaType { get; set; }

        public Track()
            : this(null)
        {
        }

        public Track(IRepository repository, string hash = null)
            : base(repository, TrackContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new TrackContent
            {
                TrackId = this.TrackId,
                Name = this.Name,
                Composer = this.Composer,
                Milliseconds = this.Milliseconds,
                Bytes = this.Bytes,
                UnitPrice = this.UnitPrice,
                MediaTypeFullHash = this.MediaType?.FullHash,
                GenreFullHash = this.Genre?.FullHash
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            if (this.Genre != null)
            {
                yield return await Task.FromResult(this.Genre);
            }
            if (this.MediaType != null)
            {
                yield return await Task.FromResult(this.MediaType);
            }
        }

        public override string UniqueId => this.TrackId.ToString();
    }

    /// <summary>
    /// VesselPosition Hash with the following included properties:
    /// 1. Genre->GenreId
    /// 2. MediaType->MediaTypeId
    /// </summary>
    public class LazyTrack : LazyHashableObject
    {
        public LazyTrack(IRepository repository, string fullHash)
            : base(repository, fullHash, Track.TrackContentType)
        {
        }

        public LazyTrack(Track track)
            : base(track, track.Genre?.LoadValue<Genre>().GenreId, track.MediaType?.LoadValue<MediaType>().MediaTypeId)
        {
        }

        public new Track LoadValue() => base.LoadValue<Track>();

        private int? _TrackId;
        public int TrackId
        {
            get
            {
                if (!_TrackId.HasValue)
                    _TrackId = int.Parse(this.UniqueId);
                return _TrackId.Value;
            }
        }

        public string GenreId => this.IncludedProperties[0];

        public string MediaTypeId => this.IncludedProperties[1];
    }
}
