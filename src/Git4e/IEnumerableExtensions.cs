using System.Collections.Generic;

namespace Git4e
{
    /// <summary>
    /// Provides extensions methods to the <see cref="System.Collections.Generic.IEnumerable{T}"/> interface.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Converts an enumeration to an instance of a <see cref="Git4e.HashableList{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The <see cref="Git4e.IHashableObject"/> derived class type of each item.</typeparam>
        /// <param name="collection">The collection to convert.</param>
        /// <param name="repository">The repository that each item belongs to.</param>
        /// <returns></returns>
        public static HashableList<T> ToHashableList<T>(this IEnumerable<T> collection, IRepository repository)
            where T: IHashableObject
        {
            return new HashableList<T>(repository, collection);
        }
    }
}
