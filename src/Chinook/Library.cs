﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace Chinook
{
    public class Library : HashableObject
    {
        public const string LibraryContentType = "Library";

        [ProtoContract]
        public class LibraryContent : IContent
        {
            [ProtoMember(1)]
            public string ArtistsHash { get; set; }
            [ProtoMember(2)]
            public string[] ArtistFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var artists =
                    this.ArtistFullHashes?
                    .Select(artistFullHash => new LazyArtist(repository, artistFullHash))
                    ?? new LazyArtist[0];
                var library = new Library(repository, hash)
                {
                    Artists = new HashableList<LazyArtist>(repository, artists, this.ArtistsHash)
                };
                return Task.FromResult(library as IHashableObject);
            }
        }

        HashableList<LazyArtist> _Artists;
        public HashableList<LazyArtist> Artists
        {
            get => _Artists;
            set
            {
                if (_Artists != value)
                {
                    _Artists = value;
                    this.MarkAsDirty();
                }
            }
        }

        public Library(IRepository repository, string hash = null)
            : base(repository, LibraryContentType, hash)
        {
        }

        protected override object GetContent()
        {
            if (this.Artists == null)
                this.Artists = new HashableList<LazyArtist>(this.Repository);

            var content = new LibraryContent
            {
                ArtistsHash = this.Artists.Hash,
                ArtistFullHashes = this.Artists.FullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var artists = this.Artists.AsEnumerable() ?? new List<LazyArtist>();
            foreach (var artist in artists)
            {
                yield return await Task.FromResult(artist);
            }
        }

        public override string UniqueId => null;
    }

    /// <summary>
    /// Library Hash with NO included properties
    /// </summary>
    public class LazyLibrary : LazyHashableObject
    {
        public LazyLibrary(IRepository repository, string fullHash)
            : base(repository, fullHash, Library.LibraryContentType)
        {
        }

        public LazyLibrary(Library library)
            : base(library)
        {
        }
    }
}
