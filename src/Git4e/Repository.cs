using System;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class Repository : IRepository
    {
        public IServiceProvider ServiceProvider { get; }
        public IObjectStore ObjectStore { get; }
        public IContentTypeResolver ContentTypeResolver { get; }

        public Repository(
            IServiceProvider serviceProvider,
            IObjectStore objectStore,
            IContentTypeResolver contentTypeResolver
            )
        {
            this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.ObjectStore = objectStore ?? throw new ArgumentNullException(nameof(objectStore));
            this.ContentTypeResolver = contentTypeResolver ?? throw new ArgumentNullException(nameof(contentTypeResolver));
        }

        public string HeadCommitHash { get; private set; }

        public async Task<ICommit> CheckoutAsync(string commitHash, CancellationToken cancellationToken = default)
        {
            var contentTypeName = await this.ObjectStore.GetObjectTypeAsync(commitHash, cancellationToken);
            if (contentTypeName != Commit.ContentTypeName)
            {
                throw new Exception("Object with hash ... is not a commit");
            }

            var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await this.ObjectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var commitContent = objectContent as IContent;
            var commit = (await commitContent.ToHashableObjectAsync(commitHash, this.ServiceProvider, cancellationToken)) as ICommit;
            if (commit != null)
            {
                this.HeadCommitHash = commitHash;
            }
            return commit;
        }

        public async Task<string> CommitAsync(string author, DateTime when, string message, IHashableObject root, CancellationToken cancellationToken = default)
        {
            Func<IHashableObject, LazyHashableObjectBase> rootCreator = Globals.RootFromInstanceCreator ?? new Func<IHashableObject, LazyHashableObjectBase>(r => new LazyHashableObject(r));

            var commit = new Commit
            {
                Author = author,
                When = when,
                Message = message,
                Root = rootCreator(root)
            };

            if (this.HeadCommitHash != null)
            {
                commit.ParentCommitHashes = new[] { this.HeadCommitHash };
            }

            await this.ObjectStore.SaveTreeAsync(commit, cancellationToken);
            await this.ObjectStore.SaveHeadAsync(commit.Hash, CancellationToken.None);

            this.HeadCommitHash = commit.Hash;
            return commit.Hash;
        }
    }
}
