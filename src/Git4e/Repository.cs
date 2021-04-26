using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <inheritdoc/>
    public class Repository : IRepository
    {
        /// <summary>
        /// A service provider that could be used to load other services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// The <see cref="Git4e.IContentSerializer"/> implementation of this repository.
        /// </summary>
        public IContentSerializer ContentSerializer { get; }

        /// <summary>
        /// The <see cref="Git4e.IHashCalculator"/> implementation of this repository.
        /// </summary>
        public IHashCalculator HashCalculator { get; }

        /// <summary>
        /// The <see cref="Git4e.IObjectStore"/> implementation of this repository.
        /// </summary>
        public IObjectStore ObjectStore { get; }

        /// <summary>
        /// The <see cref="Git4e.IContentTypeResolver"/> implementation of this repository.
        /// </summary>
        public IContentTypeResolver ContentTypeResolver { get; }

        /// <summary>
        /// The <see cref="Git4e.IRootFromHashCreator"/> implementation of this repository.
        /// </summary>
        public IRootFromHashCreator RootFromHashCreator { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.Repository"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="Git4e.IContentSerializer"/> implementation of this repository.</param>
        /// <param name="contentSerializer">The <see cref="Git4e.IContentSerializer"/> implementation of this repository.</param>
        /// <param name="hashCalculator">The <see cref="Git4e.IHashCalculator"/> implementation of this repository.</param>
        /// <param name="objectStore">The <see cref="Git4e.IObjectStore"/> implementation of this repository.</param>
        /// <param name="contentTypeResolver">The <see cref="Git4e.IContentTypeResolver"/> implementation of this repository.</param>
        /// <param name="rootFromHashCreator">The <see cref="Git4e.IRootFromHashCreator"/> implementation of this repository.</param>
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

        /// <inheritdoc/>
        public string HeadCommitHash { get; private set; }

        /// <inheritdoc/>
        public async Task<ICommit> CheckoutAsync(string branch, CancellationToken cancellationToken = default)
        {
            ICommit commit = null;

            var commitHash = await this.ObjectStore.CheckoutBranchAsync(branch, cancellationToken);
            if (commitHash != null)
            {
                var contentTypeName = await this.ObjectStore.GetContentTypeNameAsync(commitHash, cancellationToken);
                if (contentTypeName != Commit.CommitContentTypeName)
                {
                    throw new Git4eException(Git4eErrorCode.InvalidContentType, $"Object with hash '{commitHash}' is not a commit");
                }

                var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
                var commitContent = await this.ObjectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
                commit = (await commitContent.ToHashableObjectAsync(commitHash, this, cancellationToken)) as ICommit;
                if (commit != null)
                {
                    this.HeadCommitHash = commitHash;
                }
            }

            return commit;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task CreateBranchAsync(string branch, bool checkout, CancellationToken cancellationToken = default)
        {
            await this.ObjectStore.CreateBranchAsync(branch, cancellationToken);
            if (checkout)
            {
                this.HeadCommitHash = await this.ObjectStore.CheckoutBranchAsync(branch, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task InitializeRepositoryAsync(CancellationToken cancellationToken = default)
        {
            await this.ObjectStore.InitializeObjectStoreAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ICommit> GetParentCommitAsync(ICommit commit, int parentIndex = 1, CancellationToken cancellationToken = default)
        {
            if (commit is null)
            {
                throw new ArgumentNullException(nameof(commit));
            }

            if (parentIndex <= 0)
            {
                throw new ArgumentException($"{nameof(parentIndex)} must be equal or greater than 1");
            }

            if (commit.ParentCommitHashes == null)
            {
                //todo: is this correct? should we throw an error?
                return null;
            }

            if (parentIndex > commit.ParentCommitHashes.Length)
            {
                //todo: replace this exception with ArgumentOutOfRangeException?
                throw new Git4eException(Git4eErrorCode.InvalidCommitParent, $"Commit parent #{parentIndex} does not exist. Commit '{commit.Hash}' has only {commit.ParentCommitHashes} parents");
            }

            var parentCommitHash = commit.ParentCommitHashes[parentIndex - 1];
            var contentTypeName = await this.ObjectStore.GetContentTypeNameAsync(commit.Hash, cancellationToken);
            var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
            var commitContent = (await this.ObjectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken)) as IContent;
            var parentCommit = (await commitContent.ToHashableObjectAsync(parentCommitHash, this, cancellationToken)) as ICommit;
            return parentCommit;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICommit> GetCommitHistoryAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            var commitHash = this.HeadCommitHash;
            while (commitHash != null)
            {
                var contentTypeName = await this.ObjectStore.GetContentTypeNameAsync(commitHash, cancellationToken);
                var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
                var commitContent = await this.ObjectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
                var commit = (await commitContent.ToHashableObjectAsync(commitHash, this, cancellationToken)) as ICommit;
                yield return commit;

                commitHash = commit.ParentCommitHashes?.FirstOrDefault();
            }
        }
    }
}
