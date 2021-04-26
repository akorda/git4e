using System;
using System.IO;
using System.Security.Cryptography;

namespace Git4e
{
    /// <summary>
    /// Provides a method to compute the hash value for a <see cref="System.IO.Stream"/> object
    /// using the SHA1 algorithm.
    /// </summary>
    public class SHA1HashCalculator : IHashCalculator
    {
        private static readonly HashAlgorithm Sha1 = SHA1.Create();

        /// <inheritdoc/>
        public string ComputeHash(Stream stream)
        {
            var rawHash = Sha1.ComputeHash(stream);
            var hash = BitConverter.ToString(rawHash).Replace("-", "").ToLowerInvariant();
            return hash;
        }
    }
}
