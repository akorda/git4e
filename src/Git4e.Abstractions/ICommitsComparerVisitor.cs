using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Represents the visitor of a commits compare operation. See the visitor design
    /// pattern (https://en.wikipedia.org/wiki/Visitor_pattern) for more information.
    /// </summary>
    public interface ICommitsComparerVisitor
    {
        /// <summary>
        /// This method during a commits comparison is invoked when two collections are different.
        /// </summary>
        /// <param name="commit">The latest commit in the comparison.</param>
        /// <param name="collection1">The first collection.</param>
        /// <param name="collection2">The second collection.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents this operation.</returns>
        Task OnCollectionsDifferAsync(ICommit commit, IEnumerable<IHashableObject> collection1, IEnumerable<IHashableObject> collection2, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method during a commits comparison is invoked when a new hashable object is created
        /// in the latest commit.
        /// </summary>
        /// <param name="commit">The latest commit in the comparison.</param>
        /// <param name="item">The hashable object that created.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents this operation.</returns>
        Task OnItemCreatedAsync(ICommit commit, IHashableObject item, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method during a commits comparison is invoked when a hashable object is deleted
        /// in the latest commit.
        /// </summary>
        /// <param name="commit">The latest commit in the comparison.</param>
        /// <param name="item">The hashable object that deleted.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents this operation.</returns>
        Task OnItemDeletedAsync(ICommit commit, IHashableObject item, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method during a commits comparison is invoked when a hashable object is updated
        /// in the latest commit.
        /// </summary>
        /// <param name="commit">The latest commit in the comparison.</param>
        /// <param name="item1">The hashable object of the latest commit.</param>
        /// <param name="item2">The hashable object of the other commit.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents this operation.</returns>
        Task OnItemUpdatedAsync(ICommit commit, IHashableObject item1, IHashableObject item2, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method during a commits comparison is invoked when a property of a hashable object is updated
        /// in the latest commit.
        /// </summary>
        /// <param name="commit">The latest commit in the comparison.</param>
        /// <param name="propertyName">The property name that updated.</param>
        /// <param name="item1">The hashable object of the latest commit.</param>
        /// <param name="item2">The hashable object of the other commit.</param>
        /// <param name="propertyValue1">The value of the hashable object property of the latest commit.</param>
        /// <param name="propertyValue2">The value of the hashable object property of the other commit.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents this operation.</returns>
        Task OnItemPropertyUpdatedAsync(ICommit commit, string propertyName, IHashableObject item1, IHashableObject item2, object propertyValue1, object propertyValue2, CancellationToken cancellationToken = default);
    }
}
