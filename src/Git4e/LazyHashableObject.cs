using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public override string UniqueId { get; protected set; }

        protected string[] IncludedProperties { get; set; }

        public override string FullHash
        {
            get
            {
                var uniqueIdBase64 = ToBase64String(this.UniqueId);

                string includedProperties = "";
                if (this.IncludedProperties != null && this.IncludedProperties.Any())
                {
                    includedProperties = $"|{string.Join('|', this.IncludedProperties.Select(ip => ToBase64String(ip)))}";
                }

                var fullHash = $"{this.Hash}|{uniqueIdBase64}{includedProperties}";
                return fullHash;
            }
        }

        public LazyHashableObject(IRepository repository, string fullHash, string type)
            : base(async () =>
            {
                var fullHashParts = fullHash.Split('|');
                var hash = fullHashParts.First();

                var typeName = await repository.ObjectStore.GetObjectTypeAsync(hash);
                var type = repository.ContentTypeResolver.ResolveContentType(typeName);
                var objectContent = (await repository.ObjectStore.GetObjectContentAsync(hash, type)) as IContent;
                var hashableObject = await objectContent.ToHashableObjectAsync(hash, repository);
                return hashableObject;
            })
        {
            var fullHashParts = fullHash.Split('|');
            this.Hash = fullHashParts.First();

            var base64s = fullHashParts.Skip(1).Select(base64 => FromBase64String(base64)).ToArray();
            if (base64s.Length > 0)
            {
                this.UniqueId = base64s[0];

                if (base64s.Length > 1)
                {
                    this.IncludedProperties = base64s.Skip(1).ToArray();
                }
            }

            this.Type = type;
        }

        public LazyHashableObject(IHashableObject hashableObject, params string[] includedProperties)
            : base(() => hashableObject)
        {
            this.Hash = hashableObject.Hash;
            this.UniqueId = hashableObject.UniqueId;
            this.Type = hashableObject.Type;
            this.IncludedProperties = includedProperties;
        }

        private static string ToBase64String(string value)
        {
            if (value == null)
                return "NULL";

            if (value == string.Empty)
                return "";

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            return base64;
        }

        private static string FromBase64String(string base64)
        {
            if (base64 == string.Empty)
                return "";

            if (base64 == "NULL")
                return null;

            var value = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            return value;
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
}