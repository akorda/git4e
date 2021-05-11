using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class LazyHashableObject : LazyHashableObjectBase
    {
        string _Hash;
        public override string Hash
        {
            get
            {
                if (_Hash == null)
                    _Hash = this.Value.Result.Hash;
                else if (this.IsValueCreated && this.Value.Result.Hash != _Hash)
                    _Hash = this.Value.Result.Hash;
                return _Hash;
            }
            protected set
            {
                _Hash = value;
            }
        }

        public override string Type { get; protected set; }

        public override string FullHash { get => this.Hash; }

        public LazyHashableObject(IRepository repository, string hash, string type)
            : base(async () =>
            {
                var typeName = await repository.ObjectStore.GetObjectTypeAsync(hash);
                var type = repository.ContentTypeResolver.ResolveContentType(typeName);
                var objectContent = (await repository.ObjectStore.GetObjectContentAsync(hash, type)) as IContent;
                var hashableObject = await objectContent.ToHashableObjectAsync(hash, repository);
                return hashableObject;
            })
        {
            this.Hash = hash;
            this.Type = type;
        }

        public LazyHashableObject(IHashableObject hashableObject)
            : base(() => hashableObject)
        {
            this.Hash = hashableObject.Hash;
            this.Type = hashableObject.Type;
        }

        public override async Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var value = await this.Value;
            await value.SerializeContentAsync(stream, cancellationToken);
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var value = await this.Value;
            await foreach (var child in value.GetChildObjects())
                yield return child;
        }

        public override void MarkAsDirty()
        {
            this.Hash = null;
        }
    }

    public class LazyHashableObject<THashIncludeProperty1> : LazyHashableObject
    {
        public THashIncludeProperty1 HashIncludeProperty1 { get; set; }

        public LazyHashableObject(IRepository repository, string fullHash, string type)
            : base(repository, fullHash.Split('|').First(), type)
        {
            var hashParts = fullHash.Split('|');
            this.HashIncludeProperty1 = (THashIncludeProperty1)Convert.ChangeType(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(hashParts[1])), typeof(THashIncludeProperty1));
        }

        public LazyHashableObject(IHashableObject hashableObject, Func<IHashableObject, THashIncludeProperty1> hashIncludePropertyProvider)
            : base(hashableObject)
        {
            this.HashIncludeProperty1 = hashIncludePropertyProvider(hashableObject);
        }

        public override string FullHash
        {
            get
            {
                var prop1Text = this.HashIncludeProperty1?.ToString() ?? "";
                var prop1Bytes = System.Text.Encoding.UTF8.GetBytes(prop1Text);
                var hashPart1 = Convert.ToBase64String(prop1Bytes);
                return $"{this.Hash}|{hashPart1}";
            }
        }
    }

    public class LazyHashableObject<THashIncludeProperty1, THashIncludeProperty2> : LazyHashableObject
    {
        public THashIncludeProperty1 HashIncludeProperty1 { get; set; }
        public THashIncludeProperty2 HashIncludeProperty2 { get; set; }

        public LazyHashableObject(IRepository repository, string fullHash, string type)
            : base(repository, fullHash.Split('|').First(), type)
        {
            var hashParts = fullHash.Split('|');
            this.HashIncludeProperty1 = (THashIncludeProperty1)Convert.ChangeType(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(hashParts[1])), typeof(THashIncludeProperty1));
            this.HashIncludeProperty2 = (THashIncludeProperty2)Convert.ChangeType(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(hashParts[2])), typeof(THashIncludeProperty2));
        }

        public LazyHashableObject(IHashableObject hashableObject, Func<IHashableObject, THashIncludeProperty1> hashIncludeProperty1Provider, Func<IHashableObject, THashIncludeProperty2> hashIncludeProperty2Provider)
            : base(hashableObject)
        {
            this.HashIncludeProperty1 = hashIncludeProperty1Provider(hashableObject);
            this.HashIncludeProperty2 = hashIncludeProperty2Provider(hashableObject);
        }

        /// <summary>
        /// <inheritDoc />
        /// </summary>
        public override string FullHash
        {
            get
            {
                var prop1Text = this.HashIncludeProperty1?.ToString() ?? "";
                var prop1Bytes = System.Text.Encoding.UTF8.GetBytes(prop1Text);
                var hashPart1 = Convert.ToBase64String(prop1Bytes);

                var prop2Text = this.HashIncludeProperty2?.ToString() ?? "";
                var prop2Bytes = System.Text.Encoding.UTF8.GetBytes(prop2Text);
                var hashPart2 = Convert.ToBase64String(prop2Bytes);

                return $"{this.Hash}|{hashPart1}|{hashPart2}";
            }
        }
    }

    public class LazyHashableObject<THashIncludeProperty1, THashIncludeProperty2, THashIncludeProperty3> : LazyHashableObject
    {
        public THashIncludeProperty1 HashIncludeProperty1 { get; set; }
        public THashIncludeProperty2 HashIncludeProperty2 { get; set; }
        public THashIncludeProperty3 HashIncludeProperty3 { get; set; }

        public LazyHashableObject(IRepository repository, string fullHash, string type)
            : base(repository, fullHash.Split('|').First(), type)
        {
            var hashParts = fullHash.Split('|');
            this.HashIncludeProperty1 = (THashIncludeProperty1)Convert.ChangeType(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(hashParts[1])), typeof(THashIncludeProperty1));
            this.HashIncludeProperty2 = (THashIncludeProperty2)Convert.ChangeType(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(hashParts[2])), typeof(THashIncludeProperty2));
            this.HashIncludeProperty3 = (THashIncludeProperty3)Convert.ChangeType(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(hashParts[3])), typeof(THashIncludeProperty3));
        }

        public LazyHashableObject(IHashableObject hashableObject, Func<IHashableObject, THashIncludeProperty1> hashIncludeProperty1Provider, Func<IHashableObject, THashIncludeProperty2> hashIncludeProperty2Provider, Func<IHashableObject, THashIncludeProperty3> hashIncludeProperty3Provider)
            : base(hashableObject)
        {
            this.HashIncludeProperty1 = hashIncludeProperty1Provider(hashableObject);
            this.HashIncludeProperty2 = hashIncludeProperty2Provider(hashableObject);
            this.HashIncludeProperty3 = hashIncludeProperty3Provider(hashableObject);
        }

        /// <summary>
        /// <inheritDoc />
        /// </summary>
        public override string FullHash
        {
            get
            {
                var prop1Text = this.HashIncludeProperty1?.ToString() ?? "";
                var prop1Bytes = System.Text.Encoding.UTF8.GetBytes(prop1Text);
                var hashPart1 = Convert.ToBase64String(prop1Bytes);

                var prop2Text = this.HashIncludeProperty2?.ToString() ?? "";
                var prop2Bytes = System.Text.Encoding.UTF8.GetBytes(prop2Text);
                var hashPart2 = Convert.ToBase64String(prop2Bytes);

                var prop3Text = this.HashIncludeProperty3?.ToString() ?? "";
                var prop3Bytes = System.Text.Encoding.UTF8.GetBytes(prop3Text);
                var hashPart3 = Convert.ToBase64String(prop3Bytes);

                return $"{this.Hash}|{hashPart1}|{hashPart2}|{hashPart3}";
            }
        }
    }
}
