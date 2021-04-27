using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class PhysicalFilesObjectStore : IObjectStore
    {
        private const int ObjectDirLength = 2;

        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }
        public PhysicalFilesObjectStoreOptions Options { get; }

        public PhysicalFilesObjectStore(
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator,
            PhysicalFilesObjectStoreOptions options = null)
        {
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
            this.Options = options ?? new PhysicalFilesObjectStoreOptions();
        }

        public Task SaveObjectAsync(IHashableObject content, CancellationToken cancellationToken = default)
        {
            var root = this.Options.RootDirectory;
            var hash = content.Hash;
            var hashText = hash.ToString();
            var objectDirectoryName = hashText.Substring(0, ObjectDirLength);
            var objectDirectory = Path.Combine(root, objectDirectoryName);
            var filename = hashText.Substring(ObjectDirLength);
            var path = Path.Combine(objectDirectory, filename);

            if (File.Exists(path))
            {
                //fast exit. no need to write anything
                return Task.CompletedTask;
            }

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            if (!Directory.Exists(objectDirectory))
            {
                Directory.CreateDirectory(objectDirectory);
            }

            using (var stream = new FileStream(path, FileMode.CreateNew))
                content.SerializeContent(stream);

            return Task.CompletedTask;
        }

        public Task SaveObjectsAsync(IEnumerable<IHashableObject> contents, CancellationToken cancellationToken = default)
        {
            var root = this.Options.RootDirectory;
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            foreach (var content in contents)
            {
                var hash = content.Hash;
                var hashText = hash.ToString();
                var objectDirectoryName = hashText.Substring(0, ObjectDirLength);
                var objectDirectory = Path.Combine(root, objectDirectoryName);
                var filename = hashText.Substring(ObjectDirLength);
                var path = Path.Combine(objectDirectory, filename);

                if (File.Exists(path))
                {
                    //fast exit. no need to write anything
                    continue;
                }

                if (!Directory.Exists(objectDirectory))
                {
                    Directory.CreateDirectory(objectDirectory);
                }

                using (var stream = new FileStream(path, FileMode.CreateNew))
                    content.SerializeContent(stream);

                //await File.WriteAllBytesAsync(path, content.SerializeContent(contentSerializer, hashCalculator, hashToTextConverter), cancellationToken);

                //In order to preserve the "transactional" semantics of the saving contents, since
                //we wrote at least one content, we cannot cancel the request
                cancellationToken = CancellationToken.None;
            }

            return Task.CompletedTask;
        }

        public Task<string> GetObjectTypeAsync(Hash hash, CancellationToken cancellationToken = default)
        {
            var hashText = hash.ToString();
            var objectDirectoryName = hashText.Substring(0, ObjectDirLength);
            var root = this.Options.RootDirectory;
            var objectDirectory = Path.Combine(root, objectDirectoryName);
            var filename = hashText.Substring(ObjectDirLength);
            var path = Path.Combine(objectDirectory, filename);
            if (!File.Exists(path))
                throw new Exception("");//todo: object not found exception

            string type;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            //using (var reader = new BinaryReader(stream))
            //{
            //    type = reader.ReadString();
            //}
                type = this.ContentSerializer.GetObjectTypeAsync(stream);
            return Task.FromResult(type);
        }

        public Task<object> GetObjectContentAsync(Hash hash, Type contentType, CancellationToken cancellationToken = default)
        {
            var hashText = hash.ToString();
            var objectDirectoryName = hashText.Substring(0, ObjectDirLength);
            var root = this.Options.RootDirectory;
            var objectDirectory = Path.Combine(root, objectDirectoryName);
            var filename = hashText.Substring(ObjectDirLength);
            var path = Path.Combine(objectDirectory, filename);
            if (!File.Exists(path))
                throw new Exception("");//todo: object not found exception

            object content;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                content = this.ContentSerializer.DeserializeContent(stream, contentType);
            }
            return Task.FromResult(content);
        }
    }
}
