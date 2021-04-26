using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Represents the serializable content of an object
    /// </summary>
    public interface IContent
    {
        /// <summary>
        /// Provides a way to convert this content to the equivalent object instance
        /// </summary>
        /// <param name="hash">The hash of the content.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>.</returns>
        Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default);
    }
}
