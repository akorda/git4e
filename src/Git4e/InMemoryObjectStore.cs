using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class InMemoryObjectStore : IObjectStore
    {
        private class ObjectStoreItem
        {
            public string Type { get; set; }
            public byte[] RawContent { get; set; }
        }

        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }
        ConcurrentDictionary<string, ObjectStoreItem> Store { get; set; } = new ConcurrentDictionary<string, ObjectStoreItem>();

        public InMemoryObjectStore(
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator)
        {
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
        }

        public async Task SaveObjectAsync(IHashableObject content, CancellationToken cancellationToken = default)
        {
            var hash = content.Hash;
            byte[] rawContent;
            using (var stream = new MemoryStream())
            {
                await content.SerializeContentAsync(stream, cancellationToken);
                rawContent = stream.ToArray();
            }

            var item = new ObjectStoreItem
            {
                Type = content.Type,
                RawContent = rawContent
            };
            this.Store.TryAdd(hash, item);
        }

        public async Task SaveObjectsAsync(IEnumerable<IHashableObject> contents, CancellationToken cancellationToken = default)
        {
            foreach (var content in contents)
            {
                var hash = content.Hash;
                byte[] rawContent;
                using (var stream = new MemoryStream())
                {
                    await content.SerializeContentAsync(stream, cancellationToken);
                    rawContent = stream.ToArray();
                }

                var item = new ObjectStoreItem
                {
                    Type = content.Type,
                    RawContent = rawContent
                };
                this.Store.TryAdd(hash, item);

                //In order to preserve the "transactional" semantics of the saving contents, since
                //we wrote at least one content, we cannot cancel the request
                cancellationToken = CancellationToken.None;
            }
        }

        public Task<string> GetObjectTypeAsync(string hash, CancellationToken cancellationToken = default)
        {
            if (!this.Store.TryGetValue(hash, out var item))
                throw new Exception("");//todo: object not found exception

            return Task.FromResult(item.Type);
        }

        public async Task<object> GetObjectContentAsync(string hash, Type contentType, CancellationToken cancellationToken = default)
        {
            if (!this.Store.TryGetValue(hash, out var item))
                throw new Exception("");//todo: object not found exception

            object content;
            using (var stream = new MemoryStream(item.RawContent))
            {
                content = await this.ContentSerializer.DeserializeContentAsync(stream, contentType, cancellationToken);
            }
            return content;
        }
    }
}
