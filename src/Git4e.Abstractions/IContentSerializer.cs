using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Provides methods for content serialization and deserialization.
    /// </summary>
    public interface IContentSerializer
    {
        /// <summary>
        /// Serializes the content of a hashable object to a stream.
        /// </summary>
        /// <param name="stream">The stream to which the content will be serialized.</param>
        /// <param name="contentTypeName">The content type name</param>
        /// <param name="content">The content to serialize</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the serialization.</returns>
        Task SerializeContentAsync(Stream stream, string contentTypeName, object content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes the content of a hashable object from a stream.
        /// </summary>
        /// <param name="stream">The stream from which the content will be deserialized.</param>
        /// <param name="contentType">The content type</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous deserilization. The value of the TResult
        /// parameter contains the deserialized content.</returns>
        Task<IContent> DeserializeContentAsync(Stream stream, Type contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads the content type name of a hashable object from a stream.
        /// </summary>
        /// <param name="stream">The stream from which the content type name will be retrieved.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous content type name retrieval. The value of the TResult
        /// parameter contains the content type name.</returns>
        Task<string> GetContentTypeNameAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
