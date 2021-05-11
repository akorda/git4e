using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class PhysicalFilesObjectStore : IObjectStore
    {
        private const int ObjectDirLength = 2;
        private const string HeadFilename = "HEAD";

        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }
        public PhysicalFilesObjectStoreOptions Options { get; }

        public PhysicalFilesObjectStore(
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator,
            PhysicalFilesObjectStoreOptions options)
        {
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task SaveTreeAsync(IHashableObject content, CancellationToken cancellationToken = default)
        {
            var objectsDir = this.Options.ObjectsDirectory;
            var hash = content.Hash;
            var objectDirectoryName = hash.Substring(0, ObjectDirLength);
            var objectDirectory = Path.Combine(objectsDir, objectDirectoryName);
            var filename = hash.Substring(ObjectDirLength);
            var path = Path.Combine(objectDirectory, filename);

            if (File.Exists(path))
            {
                //fast exit. no need to write anything else
                return;
            }

            //todo: use logger
            //Console.WriteLine($"Saving modified content: Type {content.Type}, Hash: {content.Hash}");

            await this.SaveObjectAsync(content, cancellationToken);

            //In order to preserve the "transactional" semantics of the saving contents, since
            //we wrote at least one content, we cannot cancel the request
            cancellationToken = CancellationToken.None;

            var children = content.GetChildObjects();
            await foreach (var child in children)
                await this.SaveTreeAsync(child, cancellationToken);
        }

        public async Task SaveObjectAsync(IHashableObject content, CancellationToken cancellationToken = default)
        {
            var objectsDir = this.Options.ObjectsDirectory;
            var hash = content.Hash;
            var objectDirectoryName = hash.Substring(0, ObjectDirLength);
            var objectDirectory = Path.Combine(objectsDir, objectDirectoryName);
            var filename = hash.Substring(ObjectDirLength);
            var path = Path.Combine(objectDirectory, filename);

            if (File.Exists(path))
            {
                //fast exit. no need to write anything else
                return;
            }

            if (!Directory.Exists(objectsDir))
            {
                Directory.CreateDirectory(objectsDir);
            }

            if (!Directory.Exists(objectDirectory))
            {
                Directory.CreateDirectory(objectDirectory);
            }

            using (var stream = new FileStream(path, FileMode.CreateNew))
                await content.SerializeContentAsync(stream, cancellationToken);
        }

        public async Task SaveObjectsAsync(IEnumerable<IHashableObject> contents, CancellationToken cancellationToken = default)
        {
            var objectsDir = this.Options.ObjectsDirectory;
            if (!Directory.Exists(objectsDir))
            {
                Directory.CreateDirectory(objectsDir);
            }

            foreach (var content in contents)
            {
                var hash = content.Hash;
                var objectDirectoryName = hash.Substring(0, ObjectDirLength);
                var objectDirectory = Path.Combine(objectsDir, objectDirectoryName);
                var filename = hash.Substring(ObjectDirLength);
                var path = Path.Combine(objectDirectory, filename);

                if (File.Exists(path))
                {
                    //fast exit. no need to write anything else
                    continue;
                }

                if (!Directory.Exists(objectDirectory))
                {
                    Directory.CreateDirectory(objectDirectory);
                }

                using (var stream = new FileStream(path, FileMode.CreateNew))
                    await content.SerializeContentAsync(stream, cancellationToken);

                //In order to preserve the "transactional" semantics of the saving contents, since
                //we wrote at least one content, we cannot cancel the request
                cancellationToken = CancellationToken.None;
            }
        }

        public async Task<string> GetObjectTypeAsync(string hash, CancellationToken cancellationToken = default)
        {
            var objectDirectoryName = hash.Substring(0, ObjectDirLength);
            var objectsDir = this.Options.ObjectsDirectory;
            var objectDirectory = Path.Combine(objectsDir, objectDirectoryName);
            var filename = hash.Substring(ObjectDirLength);
            var path = Path.Combine(objectDirectory, filename);
            if (!File.Exists(path))
                throw new Exception("");//todo: object not found exception

            string type;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                type = await this.ContentSerializer.GetObjectTypeAsync(stream, cancellationToken);
            return type;
        }

        public async Task<object> GetObjectContentAsync(string hash, Type contentType, CancellationToken cancellationToken = default)
        {
            var objectDirectoryName = hash.Substring(0, ObjectDirLength);
            var objectsDir = this.Options.ObjectsDirectory;
            var objectDirectory = Path.Combine(objectsDir, objectDirectoryName);
            var filename = hash.Substring(ObjectDirLength);
            var path = Path.Combine(objectDirectory, filename);
            if (!File.Exists(path))
                throw new Exception("");//todo: object not found exception

            object content;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                content = await this.ContentSerializer.DeserializeContentAsync(stream, contentType, cancellationToken);
            }
            return content;
        }

        public async Task SaveHeadAsync(string commitHash, CancellationToken cancellationToken = default)
        {
            var root = this.Options.RootDirectory;
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            var headPath = Path.Combine(root, HeadFilename);
            //todo: currently we have only one branch. In the HEAD file we save
            //the commit hash
            var contents = commitHash;
            await File.WriteAllTextAsync(headPath, contents, cancellationToken);
        }

        public async Task<string> ReadHeadAsync(CancellationToken cancellationToken = default)
        {
            var root = this.Options.RootDirectory;

            if (!Directory.Exists(root))
                return null;

            var headPath = Path.Combine(root, HeadFilename);
            //todo: currently we have only one branch. In the HEAD file we save
            //the commit hash
            if (!File.Exists(headPath))
                return null;
            var commitHash = await File.ReadAllTextAsync(headPath, cancellationToken);
            return commitHash;
        }
    }
}
