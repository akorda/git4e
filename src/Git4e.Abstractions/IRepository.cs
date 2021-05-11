using System;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IRepository
    {
        IServiceProvider ServiceProvider { get; }
        IContentSerializer ContentSerializer { get; }
        IHashCalculator HashCalculator { get; }
        IObjectStore ObjectStore { get; }
        IContentTypeResolver ContentTypeResolver { get; }
        IRootFromHashCreator RootFromHashCreator { get; }
        string HeadCommitHash { get; }
        Task<ICommit> CheckoutAsync(string commitHash, CancellationToken cancellationToken = default);
        Task<string> CommitAsync(string author, DateTime when, string message, LazyHashableObjectBase root, CancellationToken cancellationToken = default);
    }
}
