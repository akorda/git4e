using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Git4e
{
    public class Repository : IRepository
    {
        public IObjectStore ObjectStore { get; }
        public IContentTypeResolver ContentTypeResolver { get; }
        public IObjectLoader ObjectLoader { get; }
        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }

        public Repository(
            IObjectStore objectStore,
            IContentTypeResolver contentTypeResolver,
            IObjectLoader objectLoader = null,
            IContentSerializer contentSerializer = null,
            IHashCalculator hashCalculator = null
            )
        {
            this.ObjectStore = objectStore ?? throw new ArgumentNullException(nameof(objectStore));
            this.ContentTypeResolver = contentTypeResolver ?? throw new ArgumentNullException(nameof(contentTypeResolver));
            this.ObjectLoader = objectLoader;
            this.ContentSerializer = contentSerializer;
            this.HashCalculator = hashCalculator;
        }

        public byte[] HeadCommitHash { get; private set; }

        public async Task<Commit> CheckoutAsync(byte[] commitHash, CancellationToken cancellationToken = default)
        {
            var contentTypeName = await this.ObjectStore.GetObjectTypeAsync(commitHash, cancellationToken);
            if (contentTypeName != Commit.ContentTypeName)
            {
                throw new Exception("Object with hash ... is not a commit");
            }

            var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await this.ObjectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var commitContent = objectContent as Commit.CommitContent;
            var commit = commitContent.ToHashableObject(this.ContentSerializer, this.ObjectLoader, this.HashCalculator) as Commit;
            if (commit != null)
            {
                this.HeadCommitHash = commit.Hash;
            }
            return commit;
        }

        public async Task<byte[]> CommitAsync(string author, DateTime when, string message, IHashableObject root, IEnumerable<IHashableObject> otherThanRootObjects, CancellationToken cancellationToken = default)
        {
            var commit = new Commit(this.ContentSerializer, this.HashCalculator)
            {
                Author = author,
                When = when,
                Message = message,
                Root = root
            };
            if (this.HeadCommitHash != null)
            {
                commit.ParentCommitHashes = new[] { this.HeadCommitHash };
            }

            var contents = otherThanRootObjects.Union(new[] { commit, root });
            await this.ObjectStore.SaveObjectsAsync(contents, cancellationToken);
            this.HeadCommitHash = commit.Hash;
            return commit.Hash;
        }
    }
}
