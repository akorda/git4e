using System.Collections.Generic;
using System.Threading.Tasks;

namespace Git4e
{
    public class ObjectLoader : IObjectLoader
    {
        Dictionary<string, object> ObjectCache = new Dictionary<string, object>();
        int cacheHits = 0;


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

            object obj;
            if (ObjectCache.TryGetValue(hashText, out obj))
            {
                cacheHits++;
                return obj;
            }

            var typeText = await this.ObjectStore.GetObjectTypeAsync(hash);
            var type = this.ContentTypeResolver.ResolveContentType(typeText);
            var objectContent = (await this.ObjectStore.GetObjectContentAsync(hash, type));
            if (objectContent == null)
                return null;
            var content = objectContent as IContent;

            if (content != null)
            {
                obj = content.ToObject(this.ContentSerializer, this, this.HashCalculator);
                ObjectCache[hashText] = obj;
            }
            else
            {
                obj = null;/*contentToObjectConverter(objectContent);*/
            }
            return obj;
        }
    }
}
