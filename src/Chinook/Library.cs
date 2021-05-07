using System;
using System.Collections.Generic;
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
            public string[] ArtistFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var artists =
                    this.ArtistFullHashes
                    .Select(artistHash => new LazyArtist(artistHash))
                    .ToList();
                var library = new Library(hash)
                {
                    Artists = artists
                };
                return Task.FromResult(library as IHashableObject);
            }
        }

        List<LazyArtist> _Artists;
        public List<LazyArtist> Artists
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

        public Library(string hash = null)
            : base(LibraryContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var artistFullHashes = this.Artists?
                .OrderBy(artist => artist.HashIncludeProperty1)
                .Select(artist => artist.FullHash)
                .ToArray();
            var content = new LibraryContent
            {
                ArtistFullHashes = artistFullHashes
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
    }

    /// <summary>
    /// Library Hash with NO included properties
    /// </summary>
    public class LazyLibrary : LazyHashableObject
    {
        public LazyLibrary(string fullHash)
            : base(fullHash, Library.LibraryContentType)
        {
        }

        public LazyLibrary(Library plan)
            : base(plan)
        {
        }
    }
}
