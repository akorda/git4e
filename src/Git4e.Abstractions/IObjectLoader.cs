using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Provides a method to load a hashable object that is already stored given it's hash.
    /// </summary>
    public interface IObjectLoader
    {
        /// <summary>
        /// Loads a hashable object that is already stored given it's hash.
        /// </summary>
        /// <param name="hash">The hash of the hashable object's content.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous get operation. The value of the TResult
        /// parameter contains the loaded hashable object.</returns>
        Task<IHashableObject> GetObjectByHashAsync(string hash, CancellationToken cancellationToken = default);
    }
}
