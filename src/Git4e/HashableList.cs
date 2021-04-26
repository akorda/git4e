using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Represents a collection of <see cref="Git4e.IHashableObject"/> that is maintained in sorted order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashableList<T> : SortedSet<T>, IHashableList
        where T : IHashableObject
    {
        private class HashableObjectComparer : IComparer<T>
        {
            public int Compare([AllowNull] T x, [AllowNull] T y) => string.Compare(x.UniqueId, y.UniqueId);

            public static HashableObjectComparer Instance => new HashableObjectComparer();
        }

        private string _Hash;
        private string[] _FullHashes;
        IHashCalculator HashCalculator { get; set; }

        /// <summary>
        /// Initializes a empty instance of the <see cref="Git4e.HashableList{T}"/> class.
        /// </summary>
        /// <param name="repository">The repository that all items of this collection belongs to.</param>
        public HashableList(IRepository repository)
            : base(HashableObjectComparer.Instance)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.HashCalculator = repository.HashCalculator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.HashableList{T}"/> class using an
        /// enumeration of hashable objects and their hash.
        /// </summary>
        /// <param name="repository">The repository that all items of this collection belongs to.</param>
        /// <param name="collection">The hashable objects to include in this <see cref="Git4e.HashableList{T}"/> instance.</param>
        /// <param name="hash">The hash of all hashable objects of the enumeration.</param>
        public HashableList(IRepository repository, IEnumerable<T> collection, string hash)
            : base(collection, HashableObjectComparer.Instance)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.HashCalculator = repository.HashCalculator;
            _Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.HashableList{T}"/> class using an
        /// enumeration of hashable objects.
        /// </summary>
        /// <param name="repository">The repository that all items of this collection belongs to.</param>
        /// <param name="collection">The hashable objects to include in this <see cref="Git4e.HashableList{T}"/> instance.</param>
        public HashableList(IRepository repository, IEnumerable<T> collection)
            : base(collection, HashableObjectComparer.Instance)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.HashCalculator = repository.HashCalculator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.HashableList{T}"/> class using the
        /// hash of an enumeration of hashable objects.
        /// </summary>
        /// <param name="repository">The repository that all items of this collection belongs to.</param>
        /// <param name="hash">The hash of the hashable objects.</param>
        public HashableList(IRepository repository, string hash)
            : base(HashableObjectComparer.Instance)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.HashCalculator = repository.HashCalculator;
            _Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        /// <inheritdoc/>
        public void MarkAsDirty()
        {
            _Hash = null;
            _FullHashes = null;
        }

        /// <inheritdoc/>
        public IEnumerable<IHashableObject> GetListItems()
        {
            var collection = this.Cast<IHashableObject>();
            return collection;
        }

        /// <inheritdoc/>
        public string Hash
        {
            get
            {
                if (_Hash == null)
                {
                    var hashes = (this as SortedSet<T>).Select(item => item.Hash);
                    var itemsHash = string.Join('|', hashes);
                    var itemHashesBytes = Encoding.UTF8.GetBytes(itemsHash);
                    using (var ms = new MemoryStream(itemHashesBytes))
                    {
                        _Hash = this.HashCalculator.ComputeHash(ms);
                    }
                }
                return _Hash;
            }
        }

        /// <inheritdoc/>
        public string[] FullHashes
        {
            get
            {
                if (_FullHashes == null)
                {
                    _FullHashes = (this as SortedSet<T>).Select(item => item.FullHash).ToArray();
                }
                return _FullHashes;
            }
        }
    }
}
