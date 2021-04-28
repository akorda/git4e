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
        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }

        private string _Hash;
        private byte[] _Content;

        public HashableObject(string type, IContentSerializer contentSerializer, IHashCalculator hashCalculator)
        {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
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

        public virtual IEnumerable<IHashableObject> ChildObjects { get => new IHashableObject[0]; }

        protected void MarkContentAsDirty()
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
