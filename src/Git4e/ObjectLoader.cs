using System.Collections.Generic;
using System.Threading.Tasks;

namespace Git4e
{
    public class ObjectLoader : IObjectLoader
    {
        readonly Dictionary<string, IHashableObject> ObjectCache = new Dictionary<string, IHashableObject>();

        public IHashToTextConverter HashToTextConverter { get; }
        public IObjectStore ObjectStore { get; }
        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }
        public IContentTypeResolver ContentTypeResolver { get; }
        public IContentToObjectConverter ContentToObjectConverter { get; }

        public ObjectLoader(
            IHashToTextConverter hashToTextConverter,
            IObjectStore objectStore,
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator,
            IContentTypeResolver contentTypeResolver,
            IContentToObjectConverter contentToObjectConverter = null)
        {
            this.HashToTextConverter = hashToTextConverter ?? throw new System.ArgumentNullException(nameof(hashToTextConverter));
            this.ObjectStore = objectStore ?? throw new System.ArgumentNullException(nameof(objectStore));
            this.ContentSerializer = contentSerializer ?? throw new System.ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new System.ArgumentNullException(nameof(hashCalculator));
            this.ContentTypeResolver = contentTypeResolver ?? throw new System.ArgumentNullException(nameof(contentTypeResolver));
            this.ContentToObjectConverter = contentToObjectConverter;
        }

        public async Task<IHashableObject> GetObjectByHash(byte[] hash)
        {
            if (hash == null)
                return null;

            var hashText = this.HashToTextConverter.ConvertHashToText(hash);

            if (this.ObjectCache.TryGetValue(hashText, out var obj))
            {
                return obj;
            }

            var typeText = await this.ObjectStore.GetObjectTypeAsync(hash);
            var contentType = this.ContentTypeResolver.ResolveContentType(typeText);
            var objectContent = (await this.ObjectStore.GetObjectContentAsync(hash, contentType));
            if (objectContent == null)
                return null;

            if (objectContent is IContent content)
            {
                obj = content.ToHashableObject(this.ContentSerializer, this, this.HashCalculator);
            }
            else
            {
                obj = this.ContentToObjectConverter?.ToObject(objectContent, this.ContentSerializer, this, this.HashCalculator);
            }

            this.ObjectCache[hashText] = obj;

            return obj;
        }
    }
}
