using System.IO;

namespace Git4e
{
    public interface IHashCalculator
    {
        byte[] ComputeHash(Stream stream);
    }
}
