using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class LazyHashableObject : AsyncLazy<IHashableObject>, IHashableObject
    {
        public string Hash { get; private set; }

        public string Type { get; private set; }

        public LazyHashableObject(string hash, string type)
            : base(async () =>
            {
                var typeName = await Globals.ObjectStore.GetObjectTypeAsync(hash);
                var type = Globals.ContentTypeResolver.ResolveContentType(typeName);
                var objectContent = (await Globals.ObjectStore.GetObjectContentAsync(hash, type)) as IContent;
                var hashableObject = await objectContent.ToHashableObjectAsync(hash, Globals.ServiceProvider);
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

        public async Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var value = await this.Value;
            await value.SerializeContentAsync(stream, cancellationToken);
        }

        public async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var value = await this.Value;
            await foreach (var child in value.GetChildObjects())
                yield return child;
        }
    }

    public class LazyHashableObject<T> : AsyncLazy<T>, IHashableObject
        where T: IHashableObject
    {
        public string Hash { get; private set; }

        public string Type { get; private set; }

        public LazyHashableObject(string hash, string type)
            : base(async () =>
            {
                var typeName = await Globals.ObjectStore.GetObjectTypeAsync(hash);
                var type = Globals.ContentTypeResolver.ResolveContentType(typeName);
                var objectContent = (await Globals.ObjectStore.GetObjectContentAsync(hash, type)) as IContent;
                var hashableObject = await objectContent.ToHashableObjectAsync(hash, Globals.ServiceProvider);
                return (T)hashableObject;
            })
        {
            this.Hash = hash;
            this.Type = type;
        }

        public LazyHashableObject(T hashableObject)
            : base(() => hashableObject)
        {
            this.Hash = hashableObject.Hash;
            this.Type = hashableObject.Type;
        }

        public async Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var value = await this.Value;
            await value.SerializeContentAsync(stream, cancellationToken);
        }

        public async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var value = await this.Value;
            await foreach (var child in value.GetChildObjects())
                yield return child;
        }
    }
}
