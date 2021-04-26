using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Provides access to a repository.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// A <see cref="System.IServiceProvider"/> instance that could be used to load services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// The <see cref="Git4e.IContentSerializer"/> instance used in this repository.
        /// </summary>
        IContentSerializer ContentSerializer { get; }

        /// <summary>
        /// The <see cref="Git4e.IHashCalculator"/> instance used in this repository.
        /// </summary>
        IHashCalculator HashCalculator { get; }

        /// <summary>
        /// The <see cref="Git4e.IObjectStore"/> instance used in this repository.
        /// </summary>
        IObjectStore ObjectStore { get; }

        /// <summary>
        /// The <see cref="Git4e.IContentTypeResolver"/> instance used in this repository.
        /// </summary>
        IContentTypeResolver ContentTypeResolver { get; }

        /// <summary>
        /// The <see cref="Git4e.IRootFromHashCreator"/> instance used in this repository.
        /// </summary>
        IRootFromHashCreator RootFromHashCreator { get; }

        /// <summary>
        /// The commit hash of the HEAD, or <c>null</c> if the repository is not yet checked
        /// out or there is no initial commit in the repository.
        /// </summary>
        string HeadCommitHash { get; }

        /// <summary>
        /// Checks out the specified branch.
        /// </summary>
        /// <param name="branch">The branch name to check out.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous create operation. The value of the TResult
        /// parameter is the commit object of the HEAD.</returns>
        Task<ICommit> CheckoutAsync(string branch, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new commit of the repository.
        /// </summary>
        /// <param name="author">The author of the commit.</param>
        /// <param name="when">The time that the commit occured.</param>
        /// <param name="message">The short (but probably multiline) message of the commit.</param>
        /// <param name="root">The lazy hashable object of the repository root.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous create operation. The value of the TResult
        /// parameter is the commit hash of the newly created commit.</returns>
        Task<string> CommitAsync(string author, DateTime when, string message, LazyHashableObjectBase root, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new branch of the repository.
        /// </summary>
        /// <param name="branch">The branch name to create.</param>
        /// <param name="checkout"><c>true</c> to checkout the newly created branch, or <c>false</c> otherwise.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the branch creation.</returns>
        Task CreateBranchAsync(string branch, bool checkout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes a new repository.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the initialize operation.</returns>
        Task InitializeRepositoryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets one of the parent commits of a commit.
        /// </summary>
        /// <param name="commit">The commit which the parent we want to retrieve.</param>
        /// <param name="parentIndex">1 based index of the parent commit to retrieve.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous create operation. The value of the TResult
        /// parameter is the commit of the parent.</returns>
        Task<ICommit> GetParentCommitAsync(ICommit commit, int parentIndex = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an enumeration of the commits that are part of the history of the checked-out branch.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous create operation. The value of the TResult
        /// parameter is the enumeration of the history of a branch.</returns>
        IAsyncEnumerable<ICommit> GetCommitHistoryAsync(CancellationToken cancellationToken = default);
    }
}
