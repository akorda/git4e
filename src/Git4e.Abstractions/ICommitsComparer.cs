using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Exposes a method that compares two (usually subsequent) commits
    /// </summary>
    public interface ICommitsComparer
    {
        /// <summary>
        /// Compares two commits and invokes the appropriate method of the visitor. See the visitor design
        /// pattern (https://en.wikipedia.org/wiki/Visitor_pattern) for more information.
        /// </summary>
        /// <param name="commit">The first commit to compare.</param>
        /// <param name="prevCommit">A previous commit (usually a parent commit of the first one.</param>
        /// <param name="visitor">The visitor of the algorithm.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the compare operation.</returns>
        Task CompareCommitsAsync(ICommit commit, ICommit prevCommit, ICommitsComparerVisitor visitor, CancellationToken cancellationToken = default);
    }
}