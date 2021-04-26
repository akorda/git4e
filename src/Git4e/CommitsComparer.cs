using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <inheritdoc/>
    public class CommitsComparer : ICommitsComparer
    {
        /// <inheritdoc/>
        public async Task CompareCommitsAsync(ICommit commit, ICommit prevCommit, ICommitsComparerVisitor visitor, CancellationToken cancellationToken = default)
        {
            if (commit is null)
            {
                throw new ArgumentNullException(nameof(commit));
            }

            if (prevCommit is null)
            {
                throw new ArgumentNullException(nameof(prevCommit));
            }

            if (visitor is null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            if (commit.Root.Hash == prevCommit.Root.Hash)
            {
                Console.WriteLine($"Commit '{commit.Hash}' and commit '{prevCommit.Hash}' have the same Root");
                return;
            }

            var obj1 = commit.Root.LoadValue();
            var obj2 = prevCommit.Root.LoadValue();

            await CompareObjectsAsync(commit, visitor, obj1, obj2, cancellationToken);
        }

        private static async Task CompareObjectsAsync(ICommit commit, ICommitsComparerVisitor visitor, IHashableObject obj1, IHashableObject obj2, CancellationToken cancellationToken)
        {
            var valueType = obj1.GetType();
            var valueType2 = obj2.GetType();

            if (valueType != valueType2)
            {
                //todo: add correct error message
                throw new Git4eException(Git4eErrorCode.GenericError);
            }

            var properties = valueType.GetProperties();

            //compare simple properties
            var contentProperties = properties
                .Where(prop => prop.GetCustomAttribute<ContentPropertyAttribute>() != null)
                .ToArray();

            foreach (var property in contentProperties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!object.Equals(value1, value2))
                {
                    await visitor.OnItemPropertyUpdatedAsync(commit, property.Name, obj1, obj2, value1, value2, cancellationToken);
                }
            }

            //first compare collections hashes
            var contentCollectionProperties = properties
                .Where(prop => prop.GetCustomAttribute<ContentCollectionAttribute>() != null)
                .ToArray();
            foreach (var property in contentCollectionProperties)
            {
                var collection1 = property.GetValue(obj1) as IHashableList;
                var collection2 = property.GetValue(obj2) as IHashableList;

                if (collection1.Hash != collection2.Hash)
                {
                    await visitor.OnCollectionsDifferAsync(commit, collection1.GetListItems(), collection2.GetListItems(), cancellationToken);

                    //find albums in lib1 but not in lib2 and updated albums
                    foreach (var item1 in collection1.GetListItems())
                    {
                        var item2 = collection2.GetListItems().FirstOrDefault(a => a.UniqueId == item1.UniqueId);
                        if (item2 == null)
                        {
                            await visitor.OnItemCreatedAsync(commit, item1, cancellationToken);
                            continue;
                        }

                        if (item1.Hash != item2.Hash)
                        {
                            await visitor.OnItemUpdatedAsync(commit, item1, item2, cancellationToken);

                            var value1 = item1;
                            var value2 = item2;
                            if (item1 is LazyHashableObjectBase)
                            {
                                value1 = (item1 as LazyHashableObjectBase).LoadValue();
                            }
                            if (item2 is LazyHashableObjectBase)
                            {
                                value2 = (item2 as LazyHashableObjectBase).LoadValue();
                            }

                            await CompareObjectsAsync(commit, visitor, value1, value2, cancellationToken);
                            continue;
                        }
                    }

                    foreach (var item2 in collection2.GetListItems())
                    {
                        var item1 = collection1.GetListItems().FirstOrDefault(a => a.UniqueId == item2.UniqueId);
                        if (item1 == null)
                        {
                            await visitor.OnItemDeletedAsync(commit, item2, cancellationToken);
                            continue;
                        }
                    }
                }
            }
        }
    }
}
