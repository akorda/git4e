using System;
using System.IO;
using System.Security.Cryptography;

namespace Git4e
{
    public class SHA1HashCalculator : IHashCalculator
    {
        private static readonly HashAlgorithm Sha1 = SHA1.Create();

        public string ComputeHash(Stream stream)
        {
            var rawHash = Sha1.ComputeHash(stream);
            var hash = BitConverter.ToString(rawHash).Replace("-", "").ToLowerInvariant();
            return hash;
        }
    }
}
