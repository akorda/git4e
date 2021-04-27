using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IRepository
    {
        byte[] HeadCommitHash { get; }
        Task<Commit> CheckoutAsync(byte[] commitHash, CancellationToken cancellationToken = default);
        Task<byte[]> CommitAsync(string author, DateTime when, string message, IHashableObject root, IEnumerable<IHashableObject> otherThanRootObjects, CancellationToken cancellationToken = default);
    }
}
