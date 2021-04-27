using System;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IRepository
    {
        Hash HeadCommitHash { get; }
        Task<Commit> CheckoutAsync(Hash commitHash, CancellationToken cancellationToken = default);
        Task<Hash> CommitAsync(string author, DateTime when, string message, IHashableObject root, CancellationToken cancellationToken = default);
    }
}
