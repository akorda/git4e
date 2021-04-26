using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Provides methods to save and load hashable objects.
    /// </summary>
    public interface IObjectStore
    {
        /// <summary>
        /// Saves a hashable object to the store.
        /// </summary>
        /// <param name="content">The content to save.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the save operation.</returns>
        Task SaveObjectAsync(IHashableObject content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a collection of hashable objects to the store.
        /// </summary>
        /// <param name="contents">The content to save.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the save operation.</returns>
        Task SaveObjectsAsync(IEnumerable<IHashableObject> contents, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the name of an object's content type given the specified hash.
        /// </summary>
        /// <param name="hash">The object hash.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous load operation. The value of the TResult
        /// parameter contains the content type name.</returns>
        Task<string> GetContentTypeNameAsync(string hash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads an object content from the store using it's hash and the content type.
        /// </summary>
        /// <param name="hash">The hash of the object.</param>
        /// <param name="contentType">The <see cref="System.Type"/> of the content.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous load operation. The value of the TResult
        /// parameter contains the deserialized content.</returns>
        Task<IContent> GetObjectContentAsync(string hash, Type contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the contents of an entire tree.
        /// </summary>
        /// <param name="content">The root content to save.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the save operation.</returns>
        Task SaveTreeAsync(IHashableObject content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a commit hash as the checked-out branch HEAD.
        /// </summary>
        /// <param name="commitHash">The hash of the commit to set as the active HEAD.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the save operation.</returns>
        Task SaveHeadAsync(string commitHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads the commit hash of the checked-out branch HEAD.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains the commit hash.</returns>
        Task<string> ReadHeadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Initialized the object store.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the initialization.</returns>
        Task InitializeObjectStoreAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new reference (???)
        /// </summary>
        /// <param name="referencePath"></param>
        /// <param name="commitHash">The hash of the commit to set as the active HEAD.</param>
        /// <param name="forceOverwrite"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous create operation. The value of the TResult
        /// parameter is <c>true</c> if the creation request actually created a reference or
        /// <c>false</c> otherwise.</returns>
        Task<bool> CreateReferenceAsync(string referencePath, string commitHash, bool forceOverwrite, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new branch.
        /// </summary>
        /// <param name="branch">The branch name.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the creation.</returns>
        Task CreateBranchAsync(string branch, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a value that indicates whether the specified branch exists.
        /// </summary>
        /// <param name="branch">The branch name to check.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous create operation. The value of the TResult
        /// parameter is <c>true</c> if branch exists, or <c>false</c> otherwise.</returns>
        Task<bool> BranchExistsAsync(string branch, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks out the specified branch.
        /// </summary>
        /// <param name="branch">The branch name to check out.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous create operation. The value of the TResult
        /// parameter is the commit hash of the HEAD.</returns>
        Task<string> CheckoutBranchAsync(string branch, CancellationToken cancellationToken = default);
    }
}
