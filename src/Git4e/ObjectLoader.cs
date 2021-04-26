using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <inheritdoc/>
    public class ObjectLoader : IObjectLoader
    {
        readonly Dictionary<string, IHashableObject> ObjectCache = new Dictionary<string, IHashableObject>();

        IRepository Repository { get; }
        IObjectStore ObjectStore => this.Repository.ObjectStore;
        IContentTypeResolver ContentTypeResolver => this.Repository.ContentTypeResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.ObjectLoader"/> class for a repository.
        /// </summary>
        /// <param name="repository">The associated repository.</param>
        public ObjectLoader(IRepository repository)
        {
            this.Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public async Task<IHashableObject> GetObjectByHashAsync(string hash, CancellationToken cancellationToken = default)
        {
            if (hash == null)
                return null;

            if (this.ObjectCache.TryGetValue((string)hash, out var obj))
            {
                return obj;
            }

            var contentTypeName = await this.ObjectStore.GetContentTypeNameAsync(hash, cancellationToken);
            var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
            var content = (await this.ObjectStore.GetObjectContentAsync(hash, contentType, cancellationToken));
            if (content == null)
                return null;

            obj = await content.ToHashableObjectAsync(hash, this.Repository, cancellationToken);

            this.ObjectCache[hash] = obj;

            return obj;
        }
    }
}
