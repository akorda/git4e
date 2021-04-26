using System.Collections.Generic;

namespace Git4e
{
    /// <summary>
    /// Represents a collection of hashable objects. It provides properties and methods so that
    /// you could determine if the collection has changed from a previous commit without loading
    /// it's items.
    /// </summary>
    public interface IHashableList
    {
        /// <summary>
        /// The generated hash of the content.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// The full hashes of the collection items.
        /// </summary>
        string[] FullHashes { get; }

        /// <summary>
        /// Forces the object to reevaluate the content and by extension the hash of the
        /// object. Invoke this method when a property of the object has changed.
        /// </summary>
        void MarkAsDirty();

        /// <summary>
        /// Gets the collection items.
        /// </summary>
        /// <returns>An enumeration of the collection items.</returns>
        IEnumerable<IHashableObject> GetListItems();
    }
}
