using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <inheritdoc/>
    public class ConsoleCommitsComparerVisitor : ICommitsComparerVisitor
    {
        /// <inheritdoc/>
        public Task OnCollectionsDifferAsync(ICommit commit, IEnumerable<IHashableObject> collection1, IEnumerable<IHashableObject> collection2, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnItemCreatedAsync(ICommit commit, IHashableObject item, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Item with {nameof(IHashableObject.ContentTypeName)} '{item.ContentTypeName}', {nameof(IHashableObject.Hash)} '{item.Hash}', {nameof(IHashableObject.UniqueId)} '{item.UniqueId}' created at commit '{commit.Hash}'");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnItemDeletedAsync(ICommit commit, IHashableObject item, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Item with {nameof(IHashableObject.ContentTypeName)} '{item.ContentTypeName}', {nameof(IHashableObject.Hash)} '{item.Hash}', {nameof(IHashableObject.UniqueId)} '{item.UniqueId}' deleted at commit '{commit.Hash}'");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnItemPropertyUpdatedAsync(ICommit commit, string propertyName, IHashableObject item1, IHashableObject item2, object propertyValue1, object propertyValue2, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Property '{propertyName}' of Item with {nameof(IHashableObject.ContentTypeName)} '{item1.ContentTypeName}', {nameof(IHashableObject.Hash)} '{item1.Hash}', {nameof(IHashableObject.UniqueId)} '{item1.UniqueId}' updated to item with {nameof(IHashableObject.Hash)} '{item2.Hash}', {nameof(IHashableObject.UniqueId)} '{item2.UniqueId} and values '{propertyValue1}' -> '{propertyValue2}' at commit '{commit.Hash}'");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnItemUpdatedAsync(ICommit commit, IHashableObject item1, IHashableObject item2, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Item with {nameof(IHashableObject.ContentTypeName)} '{item1.ContentTypeName}', {nameof(IHashableObject.Hash)} '{item1.Hash}', {nameof(IHashableObject.UniqueId)} '{item1.UniqueId}' updated to item with {nameof(IHashableObject.Hash)} '{item2.Hash}', {nameof(IHashableObject.UniqueId)} '{item2.UniqueId} at commit '{commit.Hash}'");
            return Task.CompletedTask;
        }
    }
}
