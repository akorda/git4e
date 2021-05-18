using System.Collections.Generic;

namespace Git4e
{
    public static class IEnumerableExtensions
    {
        public static HashableList<T> ToHashableList<T>(this IEnumerable<T> collection, IRepository repository)
            where T: IHashableObject
        {
            return new HashableList<T>(repository, collection);
        }
    }
}
