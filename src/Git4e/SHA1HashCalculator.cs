using System.IO;
using System.Security.Cryptography;

namespace Git4e
{
    public class SHA1HashCalculator : IHashCalculator
    {
        private static readonly HashAlgorithm Sha1 = SHA1.Create();

        public Hash ComputeHash(Stream stream)
        {
            return Sha1.ComputeHash(stream);
        }
    }
}
