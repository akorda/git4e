using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface ICommitsComparerVisitor
    {
        Task OnCollectionsDifferAsync(ICommit commit, IEnumerable<IHashableObject> collection1, IEnumerable<IHashableObject> collection2, CancellationToken cancellationToken = default);
        Task OnItemCreatedAsync(ICommit commit, IHashableObject item, CancellationToken cancellationToken = default);
        Task OnItemDeletedAsync(ICommit commit, IHashableObject item, CancellationToken cancellationToken = default);
        Task OnItemUpdatedAsync(ICommit commit, IHashableObject item1, IHashableObject item2, CancellationToken cancellationToken = default);
        Task OnItemPropertyUpdatedAsync(ICommit commit, string propertyName, IHashableObject item1, IHashableObject item2, object propertyValue1, object propertyValue2, CancellationToken cancellationToken = default);
    }
}
