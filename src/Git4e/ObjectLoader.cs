using System.Collections.Generic;
using System.Threading.Tasks;

namespace Git4e
{
    public class ObjectLoader : IObjectLoader
    {
        readonly Dictionary<string, object> ObjectCache = new Dictionary<string, object>();

        public IHashToTextConverter HashToTextConverter { get; }
        public IObjectStore ObjectStore { get; }
        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }
        public IContentTypeResolver ContentTypeResolver { get; }

        public ObjectLoader(
            IHashToTextConverter hashToTextConverter,
            IObjectStore objectStore,
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator,
            IContentTypeResolver contentTypeResolver)
        {
            this.HashToTextConverter = hashToTextConverter ?? throw new System.ArgumentNullException(nameof(hashToTextConverter));
            this.ObjectStore = objectStore ?? throw new System.ArgumentNullException(nameof(objectStore));
            this.ContentSerializer = contentSerializer ?? throw new System.ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new System.ArgumentNullException(nameof(hashCalculator));
            this.ContentTypeResolver = contentTypeResolver ?? throw new System.ArgumentNullException(nameof(contentTypeResolver));
        }

        public async Task<object> GetObjectByHash(byte[] hash)
        {
            if (hash == null)
                return null;

            var hashText = this.HashToTextConverter.ConvertHashToText(hash);

            if (this.ObjectCache.TryGetValue(hashText, out var obj))
            {
                return obj;
            }

            var typeText = await this.ObjectStore.GetObjectTypeAsync(hash);
            var type = this.ContentTypeResolver.ResolveContentType(typeText);
            var objectContent = (await this.ObjectStore.GetObjectContentAsync(hash, type));
            if (objectContent == null)
                return null;

            if (objectContent is IContent content)
            {
                obj = content.ToHashableObject(this.ContentSerializer, this, this.HashCalculator);
                this.ObjectCache[hashText] = obj;
            }
            else
            {
                obj = null;/*contentToObjectConverter(objectContent);*/
            }
            return obj;
        }
    }
}
