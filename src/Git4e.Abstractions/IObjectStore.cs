using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IObjectStore
    {
        Task SaveObjectAsync(IHashableObject content, CancellationToken cancellationToken = default);
        Task SaveObjectsAsync(IEnumerable<IHashableObject> contents, CancellationToken cancellationToken = default);
        Task<string> GetObjectTypeAsync(string hash, CancellationToken cancellationToken = default);
        Task<object> GetObjectContentAsync(string hash, Type contentType, CancellationToken cancellationToken = default);
        Task SaveTreeAsync(IHashableObject content, CancellationToken cancellationToken = default);
        Task SaveHeadAsync(string commitHash, CancellationToken cancellationToken = default);
        Task<string> ReadHeadAsync(CancellationToken cancellationToken = default);
        Task InitializeObjectStoreAsync(CancellationToken cancellationToken = default);
        Task<bool> CreateReferenceAsync(string referencePath, string commitHash, bool forceOverwrite, CancellationToken cancellationToken = default);
        Task CreateBranchAsync(string branch, CancellationToken cancellationToken = default);
        Task<bool> BranchExistsAsync(string branch, CancellationToken cancellationToken = default);
        Task<string> CheckoutBranchAsync(string branch, CancellationToken cancellationToken = default);
    }
}
