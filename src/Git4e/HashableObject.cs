using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Git4e
{
    [DebuggerDisplay("{Type,nq}, {_Hash,nq}")]
    public abstract class HashableObject : IHashableObject
    {
        public string Type { get; private set; }
        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }

        public HashableObject(string type, IContentSerializer contentSerializer, IHashCalculator hashCalculator)
        {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
        }

        public abstract void SerializeContent(Stream stream);

        public virtual IEnumerable<IHashableObject> ChildObjects { get => new IHashableObject[0]; }

        protected void MarkContentAsDirty()
        {
            _Hash = null;
        }

        private string _Hash;
        public virtual string Hash
        {
            get
            {
                if (_Hash == null)
                {
                    using (var stream = new MemoryStream())
                    {
                        this.SerializeContent(stream);
                        stream.Position = 0;
                        _Hash = this.HashCalculator.ComputeHash(stream);
                    }
                }
                return _Hash;
            }
        }
    }
}
