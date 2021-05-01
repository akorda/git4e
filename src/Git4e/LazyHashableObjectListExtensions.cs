using System.Collections.Generic;

namespace Git4e
{
    public static class LazyHashableObjectListExtensions
    {
        public static LazyHashableObjectList<T> ToLazyHashableObjectList<T>(this IEnumerable<LazyHashableObject<T>> collection)
            where T : IHashableObject
        {
            return new LazyHashableObjectList<T>(collection);
        }
    }
}
