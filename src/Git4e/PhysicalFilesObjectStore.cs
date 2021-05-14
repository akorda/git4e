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

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(this.Options.RootDirectory))
            {
                //todo: log me
                Directory.CreateDirectory(this.Options.RootDirectory);
            }

            if (!Directory.Exists(this.Options.ObjectsDirectory))
            {
                //todo: log me
                Directory.CreateDirectory(this.Options.ObjectsDirectory);
            }

            //set head to point to the initial branch
            var headPath = Path.Combine(this.Options.RootDirectory, HeadFilename);
            if (!File.Exists(headPath))
            {
                //set HEAD ref to InitialBranch even if this reference does not exist
                var relativeReferencePath = $"refs/heads/{this.Options.InitialBranchName}";
                var headContents = $"ref: {relativeReferencePath}";
                await File.WriteAllTextAsync(headPath, headContents);
            }
        }

        private static string ToOSPath(string path)
        {
            if (Path.DirectorySeparatorChar == '/') return path;
            return path.Replace('/', Path.DirectorySeparatorChar);
        }

        public async Task<bool> CreateReferenceAsync(string relativeReferencePath, string commitHash, bool forceOverwrite, CancellationToken cancellationToken = default)
        {
            var osRelativeReferencePath = ToOSPath(relativeReferencePath);
            var fullReferencePath = Path.Combine(this.Options.RootDirectory, osRelativeReferencePath);

            if (File.Exists(fullReferencePath) && !forceOverwrite)
            {
                //todo: log me
                return false;
            }

            var fullReferenceDir = Path.GetDirectoryName(fullReferencePath);
            if (!Directory.Exists(fullReferenceDir))
            {
                //todo: log me
                Directory.CreateDirectory(fullReferenceDir);
            }

            if (commitHash == null) commitHash = "";
            await File.WriteAllTextAsync(fullReferencePath, commitHash, cancellationToken);

            return true;
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

            await this.InitializeAsync(cancellationToken);

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
            await this.InitializeAsync(cancellationToken);

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
            await this.InitializeAsync(cancellationToken);

            var headPath = Path.Combine(this.Options.RootDirectory, HeadFilename);
            var headContents = await File.ReadAllTextAsync(headPath, cancellationToken);
            if (headContents.StartsWith("ref: "))
            {
                var referencePath = headContents.Substring("ref: ".Length);
                var osReferencePath = ToOSPath(referencePath);
                var fullReferencePath = Path.Combine(this.Options.RootDirectory, osReferencePath);
                //ensure that the ref directory is created
                var fullReferenceDir = Path.GetDirectoryName(fullReferencePath);
                if (!Directory.Exists(fullReferenceDir))
                {
                    Directory.CreateDirectory(fullReferenceDir);
                }

                await File.WriteAllTextAsync(fullReferencePath, commitHash, cancellationToken);
            }
            else
            {
                await File.WriteAllTextAsync(headPath, commitHash, cancellationToken);
            }
        }

        public async Task<string> ReadHeadAsync(CancellationToken cancellationToken = default)
        {
            var headPath = Path.Combine(this.Options.RootDirectory, HeadFilename);

            if (!File.Exists(headPath))
                return null;

            await this.InitializeAsync(cancellationToken);

            var headContents = await File.ReadAllTextAsync(headPath, cancellationToken);

            string commitHash;
            if (headContents.StartsWith("ref: "))
            {
                var referencePath = headContents.Substring("ref: ".Length);
                var osReferencePath = ToOSPath(referencePath);
                var fullReferencePath = Path.Combine(this.Options.RootDirectory, osReferencePath);
                if (!File.Exists(fullReferencePath))
                {
                    commitHash = null;
                }
                else
                {
                    commitHash = await File.ReadAllTextAsync(fullReferencePath, cancellationToken);
                    if (commitHash == string.Empty)
                    {
                        commitHash = null;
                    }
                }
            }
            else
            {
                commitHash = headContents;
            }
            return commitHash;
        }

        public async Task CreateBranchAsync(string branch, CancellationToken cancellationToken = default)
        {
            await this.InitializeAsync(cancellationToken);

            var branchExists = await this.BranchExistsAsync(branch, cancellationToken);
            if (branchExists)
            {
                throw new Git4eException($"A branch named '{branch}' already exists.");
            }

            var commitHash = await this.ReadHeadAsync(cancellationToken);
            if (commitHash == null)
            {
                //since there is no initial commit we don't need to
                //create the reference file

                //todo: log me
                return;
            }

            //todo: change to method
            var relativeReferencePath = $"refs/heads/{branch}";
            var forceOverwrite = false;
            await this.CreateReferenceAsync(relativeReferencePath, commitHash, forceOverwrite, cancellationToken);
        }

        public Task<bool> BranchExistsAsync(string branch, CancellationToken cancellationToken = default)
        {
            var relativeReferencePath = $"refs/heads/{branch}";
            var osRelativeReferencePath = ToOSPath(relativeReferencePath);
            var fullReferencePath = Path.Combine(this.Options.RootDirectory, osRelativeReferencePath);

            var branchExists = File.Exists(fullReferencePath);
            return Task.FromResult(branchExists);
        }

        public async Task<string> CheckoutBranchAsync(string branch, CancellationToken cancellationToken = default)
        {
            await this.InitializeAsync(cancellationToken);

            //set head to point to this branch
            var relativeReferencePath = $"refs/heads/{branch}";
            var osRelativeReferencePath = ToOSPath(relativeReferencePath);
            var fullReferencePath = Path.Combine(this.Options.RootDirectory, osRelativeReferencePath);
            string commitHash = null;
            if (File.Exists(fullReferencePath))
            {
                commitHash = await File.ReadAllTextAsync(fullReferencePath, cancellationToken);
                if (commitHash == string.Empty)
                {
                    commitHash = null;
                }
            }

            var headPath = Path.Combine(this.Options.RootDirectory, HeadFilename);
            var headContents = $"ref: {relativeReferencePath}";
            await File.WriteAllTextAsync(headPath, headContents);

            return commitHash;
        }
    }
}
