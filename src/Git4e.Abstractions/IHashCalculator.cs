using System.IO;

namespace Git4e
{
    /// <summary>
    /// Provides a method to compute the hash value for a <see cref="System.IO.Stream"/> object.
    /// </summary>
    public interface IHashCalculator
    {
        /// <summary>
        /// Computes the hash value for the specified <see cref="System.IO.Stream"/> object.
        /// </summary>
        /// <param name="stream">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        string ComputeHash(Stream stream);
    }
}
