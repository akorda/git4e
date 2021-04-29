using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class ObjectLoader : IObjectLoader
    {
        readonly Dictionary<string, IHashableObject> ObjectCache = new Dictionary<string, IHashableObject>();

        public IServiceProvider ServiceProvider { get; }
        public IObjectStore ObjectStore { get; }
        public IContentSerializer ContentSerializer { get; }
        public IContentTypeResolver ContentTypeResolver { get; }
        public IContentToObjectConverter ContentToObjectConverter { get; }

        public ObjectLoader(
            IServiceProvider serviceProvider,
            IObjectStore objectStore,
            IContentSerializer contentSerializer,
            IContentTypeResolver contentTypeResolver,
            IContentToObjectConverter contentToObjectConverter = null)
        {
            this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.ObjectStore = objectStore ?? throw new ArgumentNullException(nameof(objectStore));
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
            this.ContentTypeResolver = contentTypeResolver ?? throw new ArgumentNullException(nameof(contentTypeResolver));
            this.ContentToObjectConverter = contentToObjectConverter;
        }

        public async Task<IHashableObject> GetObjectByHashAsync(string hash, CancellationToken cancellationToken = default)
        {
            if (hash == null)
                return null;

            if (this.ObjectCache.TryGetValue((string)hash, out var obj))
            {
                return obj;
            }

            var typeText = await this.ObjectStore.GetObjectTypeAsync(hash, cancellationToken);
            var contentType = this.ContentTypeResolver.ResolveContentType(typeText);
            var objectContent = (await this.ObjectStore.GetObjectContentAsync(hash, contentType, cancellationToken));
            if (objectContent == null)
                return null;

            if (objectContent is IContent content)
            {
                obj = await content.ToHashableObjectAsync(hash, this.ServiceProvider, cancellationToken);
            }
            else
            {
                obj = this.ContentToObjectConverter?.ToObject(objectContent, this.ServiceProvider, this.ContentSerializer, this);
            }

            this.ObjectCache[hash] = obj;

            return obj;
        }
    }
}
