using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class Repository : IRepository
    {
        public IServiceProvider ServiceProvider { get; }
        public IContentSerializer ContentSerializer { get; }
        public IHashCalculator HashCalculator { get; }
        public IObjectStore ObjectStore { get; }
        public IContentTypeResolver ContentTypeResolver { get; }
        public IRootFromHashCreator RootFromHashCreator { get; }

        public Repository(
            IServiceProvider serviceProvider,
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator,
            IObjectStore objectStore,
            IContentTypeResolver contentTypeResolver,
            IRootFromHashCreator rootFromHashCreator)
        {
            this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
            this.HashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
            this.ObjectStore = objectStore ?? throw new ArgumentNullException(nameof(objectStore));
            this.ContentTypeResolver = contentTypeResolver ?? throw new ArgumentNullException(nameof(contentTypeResolver));
            this.RootFromHashCreator = rootFromHashCreator ?? throw new ArgumentNullException(nameof(rootFromHashCreator));
        }

        public string HeadCommitHash { get; private set; }

        public async Task<ICommit> CheckoutAsync(string branch, CancellationToken cancellationToken = default)
        {
            ICommit commit = null;

            var commitHash = await this.ObjectStore.CheckoutBranchAsync(branch, cancellationToken);
            if (commitHash != null)
            {
                var contentTypeName = await this.ObjectStore.GetObjectTypeAsync(commitHash, cancellationToken);
                if (contentTypeName != Commit.ContentTypeName)
                {
                    throw new Git4eException(Git4eErrorCode.InvalidContentType, $"Object with hash '{commitHash}' is not a commit");
                }

                var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
                var objectContent = await this.ObjectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
                var commitContent = objectContent as IContent;
                commit = (await commitContent.ToHashableObjectAsync(commitHash, this, cancellationToken)) as ICommit;
                if (commit != null)
                {
                    this.HeadCommitHash = commitHash;
                }
            }

            return commit;
        }

        public async Task<string> CommitAsync(string author, DateTime when, string message, LazyHashableObjectBase root, CancellationToken cancellationToken = default)
        {
            var commit = new Commit(this)
            {
                Author = author,
                When = when,
                Message = message,
                Root = root
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

        public async Task CreateBranchAsync(string branch, bool checkout, CancellationToken cancellationToken = default)
        {
            await this.ObjectStore.CreateBranchAsync(branch, cancellationToken);
            if (checkout)
            {
                this.HeadCommitHash = await this.ObjectStore.CheckoutBranchAsync(branch, cancellationToken);
            }
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await this.ObjectStore.InitializeAsync(cancellationToken);
        }

        public async Task<ICommit> GetParentCommitAsync(ICommit commit, CancellationToken cancellationToken = default)
        {
            if (commit is null)
            {
                throw new ArgumentNullException(nameof(commit));
            }

            var parentCommitHash = commit.ParentCommitHashes?.FirstOrDefault();
            if (parentCommitHash == null)
            {
                return null;
            }

            var contentTypeName = await this.ObjectStore.GetObjectTypeAsync(commit.Hash, cancellationToken);
            var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
            var commitContent = (await this.ObjectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken)) as IContent;
            var parentCommit = (await commitContent.ToHashableObjectAsync(parentCommitHash, this, cancellationToken)) as ICommit;
            return parentCommit;
        }
    }
}
