using System;
using System.Threading.Tasks;

namespace Git4e
{
    public class Repository : IRepository
    {
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

        public IObjectStore ObjectStore { get; }
        public IContentTypeResolver ContentTypeResolver { get; }
        public IObjectLoader ObjectLoader { get; }
        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }

        public async Task<Commit> CheckoutAsync(byte[] commitHash)
        {
            var contentTypeName = await this.ObjectStore.GetObjectTypeAsync(commitHash);
            if (contentTypeName != Commit.ContentTypeName)
            {
                throw new Exception("Object with hash ... is not a commit");
            }

            var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await this.ObjectStore.GetObjectContentAsync(commitHash, contentType);
            var commitContent = objectContent as Commit.CommitContent;
            var commit = commitContent.ToHashableObject(this.ContentSerializer, this.ObjectLoader, this.HashCalculator) as Commit;
            return commit;
        }
    }
}
