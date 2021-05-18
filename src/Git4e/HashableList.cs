using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Git4e
{
    public class HashableList<T> : SortedSet<T>
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

        public HashableList(IRepository repository)
            : base(HashableObjectComparer.Instance)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.HashCalculator = repository.HashCalculator;
        }

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

        public HashableList(IRepository repository, IEnumerable<T> collection)
            : base(collection, HashableObjectComparer.Instance)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.HashCalculator = repository.HashCalculator;
        }

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

        public void MarkAsDirty()
        {
            _Hash = null;
            _FullHashes = null;
        }

        public string Hash
        {
            get
            {
                if (_Hash == null)
                {
                    var hashes = this.Select(item => item.Hash);
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

        public string[] FullHashes
        {
            get
            {
                if (_FullHashes == null)
                {
                    _FullHashes = this.Select(item => item.FullHash).ToArray();
                }
                return _FullHashes;
            }
        }
    }
}
