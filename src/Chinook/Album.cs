﻿using System;
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
        public const string AlbumContentType = "Album";

        [ProtoContract]
        public class AlbumContent : IContent
        {
            [ProtoMember(1)]
            public int AlbumId { get; set; }
            [ProtoMember(2)]
            public string Title { get; set; }
            [ProtoMember(3)]
            public string[] TrackFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var tracks =
                    this.TrackFullHashes
                    .Select(posHash => new LazyTrack(posHash))
                    .ToList();
                var album = new Album(hash)
                {
                    AlbumId = this.AlbumId,
                    Title = this.Title,
                    Tracks = tracks
                };
                return Task.FromResult(album as IHashableObject);
            }
        }

        int _AlbumId;
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

        List<LazyTrack> _Tracks;
        public List<LazyTrack> Tracks
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

        public Album(string hash = null)
            : base(AlbumContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var trackFullHashes = this.Tracks?
                .OrderBy(t => t.HashIncludeProperty1)
                .Select(t => t.FullHash)
                .ToArray();
            var content = new AlbumContent
            {
                AlbumId = this.AlbumId,
                Title = this.Title,
                TrackFullHashes = trackFullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var tracks = this.Tracks ?? new List<LazyTrack>();
            foreach (var track in tracks)
            {
                yield return await Task.FromResult(track);
            }
        }
    }

    /// <summary>
    /// Album Hash with the following included properties:
    /// 1. AlbumId
    /// 2. Album Name
    /// </summary>
    public class LazyAlbum : LazyHashableObject<int>
    {
        public LazyAlbum(string fullHash)
            : base(fullHash, Album.AlbumContentType)
        {
        }

        public LazyAlbum(Album album)
            : base(album, a => (a as Album).AlbumId)
        {
        }
    }
}