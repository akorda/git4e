using System;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IRepository
    {
        string HeadCommitHash { get; }
        Task<ICommit> CheckoutAsync(string commitHash, CancellationToken cancellationToken = default);
        Task<string> CommitAsync(string author, DateTime when, string message, IHashableObject root, CancellationToken cancellationToken = default);
    }
}
