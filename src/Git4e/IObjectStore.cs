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
    }
}
