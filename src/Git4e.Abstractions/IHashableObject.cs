using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Represents an object that can be serialized and produce a unique hash.
    /// </summary>
    public interface IHashableObject
    {
        /// <summary>
        /// The content type name of the object
        /// </summary>
        string ContentTypeName { get; }

        /// <summary>
        /// The generated hash of the content.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// The unique id of the object.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// Concatenation of the Hash, the Unique Id and any included properties.
        /// </summary>
        string FullHash { get; }

        /// <summary>
        /// Serialized the object content to the specified stream
        /// </summary>
        /// <param name="stream">An opened stream to serialize the object content.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous serialization.</returns>
        Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an asynchronous collection of all immediate child objects. A child object
        /// could be a LazyHashableObject.
        /// </summary>
        /// <returns>The asynchronous collection of the child items.</returns>
        IAsyncEnumerable<IHashableObject> GetChildObjects();

        /// <summary>
        /// Forces the object to reevaluate the content and by extension the hash of the
        /// object. Invoke this method when a property of the object has changed.
        /// </summary>
        void MarkAsDirty();
    }
}
