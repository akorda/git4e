using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Provides a base class of all hashable objects. This is an abstract class.
    /// </summary>
    [DebuggerDisplay("{Type,nq}, {_Hash,nq}")]
    public abstract class HashableObject : IHashableObject
    {
        /// <inheritdoc/>
        public string ContentTypeName { get; private set; }

        /// <inheritdoc/>
        public abstract string UniqueId { get; }

        /// <summary>
        /// The repository that this object belongs
        /// </summary>
        public IRepository Repository { get; set; }

        IContentSerializer ContentSerializer { get => this.Repository.ContentSerializer; }
        IHashCalculator HashCalculator { get => this.Repository.HashCalculator; }

        private string _Hash;
        private byte[] _Content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.HashableObject"/> class. Sets the repository that
        /// this object belongs, the content type name and probably it's hash
        /// </summary>
        /// <param name="repository">The repository that this object belongs.</param>
        /// <param name="contentTypeName">The content type name of this object.</param>
        /// <param name="hash">The hash of the object. If the hash is null it will be evaluated later on,
        /// using the properties of the object.</param>
        public HashableObject(IRepository repository, string contentTypeName, string hash = null)
        {
            //todo: check for null repo?
            this.Repository = repository;
            this.ContentTypeName = contentTypeName ?? throw new ArgumentNullException(nameof(contentTypeName));
            _Hash = hash;
        }

        /// <inheritdoc/>
        public virtual async Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (_Content == null)
            {
                var content = this.GetContent();
                using (var ms = new MemoryStream())
                {
                    await this.ContentSerializer.SerializeContentAsync(ms, this.ContentTypeName, content, cancellationToken);
                    _Content = ms.ToArray();
                }
            }

            var writer = new BinaryWriter(stream);
            writer.Write(_Content);
        }

        /// <summary>
        /// Provides the content instance of the object
        /// </summary>
        /// <returns>The content of this object.</returns>
        protected abstract IContent GetContent();

        /// <inheritdoc/>
        public async virtual IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            foreach (var child in new IHashableObject[0])
            {
                var ch = await Task.FromResult(child);
                yield return ch;
            }
        }

        /// <inheritdoc/>
        public void MarkAsDirty()
        {
            _Content = null;
            _Hash = null;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual string FullHash { get => this.Hash; }
    }
}
