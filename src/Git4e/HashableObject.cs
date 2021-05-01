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
        public IContentSerializer ContentSerializer { get; } = Globals.ContentSerializer;
        public IHashCalculator HashCalculator { get; } = Globals.HashCalculator;
        public IObjectStore ObjectStore { get; set; } = Globals.ObjectStore;
        public IServiceProvider ServiceProvider { get; } = Globals.ServiceProvider;
        public IObjectLoader ObjectLoader { get; set; } = Globals.ObjectLoader;
        public IContentTypeResolver ContentTypeResolver { get; set; } = Globals.ContentTypeResolver;

        private string _Hash;
        private byte[] _Content;

        public HashableObject(string type, string hash = null)
        {
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
    }
}
