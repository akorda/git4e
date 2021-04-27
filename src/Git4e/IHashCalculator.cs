using System.IO;

namespace Git4e
{
    public interface IHashCalculator
    {
        Hash ComputeHash(Stream stream);
    }
}
