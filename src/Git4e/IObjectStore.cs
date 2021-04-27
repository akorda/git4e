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
        Task<string> GetObjectTypeAsync(Hash hash, CancellationToken cancellationToken = default);
        Task<object> GetObjectContentAsync(Hash hash, Type contentType, CancellationToken cancellationToken = default);
        Task<Hash> AddCommit(Commit commit, CancellationToken cancellationToken = default);
    }
}
