using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    [DebuggerDisplay("{Type,nq}, {_Hash,nq}")]
    public abstract class HashableObject : IHashableObject
    {
        public string Type { get; private set; }
        public abstract string UniqueId { get; }
        public IRepository Repository {get; set; }
        public IContentSerializer ContentSerializer { get => this.Repository.ContentSerializer; }
        public IHashCalculator HashCalculator { get => this.Repository.HashCalculator; }
        public IObjectStore ObjectStore { get => this.Repository.ObjectStore; }
        public IServiceProvider ServiceProvider { get => this.Repository.ServiceProvider; }
        public IContentTypeResolver ContentTypeResolver { get => this.Repository.ContentTypeResolver; }

        private string _Hash;
        private byte[] _Content;

        public HashableObject(IRepository repository, string type, string hash = null)
        {
            this.Repository = repository;
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            _Hash = hash;
        }

        public virtual async Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (_Content == null)
            {
                var content = this.GetContent();
                using (var ms = new MemoryStream())
                {
                    await this.ContentSerializer.SerializeContentAsync(ms, this.Type, content, cancellationToken);
                    _Content = ms.ToArray();
                }
            }

            var writer = new BinaryWriter(stream);
            writer.Write(_Content);
        }

        protected abstract object GetContent();

        public async virtual IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            foreach (var child in new IHashableObject[0])
            {
                var ch = await Task.FromResult(child);
                yield return ch;
            }
        }

        public void MarkAsDirty()
        {
            _Content = null;
            _Hash = null;
        }

        public virtual string Hash
        {
            get
            {
                if (_Hash == null)
                {
                    using (var stream = new MemoryStream())
                    {
                        this.SerializeContentAsync(stream, CancellationToken.None).Wait();
                        stream.Position = 0;
                        _Hash = this.HashCalculator.ComputeHash(stream);
                    }
                }
                return _Hash;
            }
        }

        public virtual string FullHash { get => this.Hash; }
    }
}
