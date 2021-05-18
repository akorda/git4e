using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface ICommitsComparer
    {
        Task CompareCommits(ICommit commit, ICommit prevCommit, ICommitsComparerVisitor visitor, CancellationToken cancellationToken = default);
    }
}