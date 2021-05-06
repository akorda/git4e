using System.IO;

namespace Git4e
{
    public interface IHashCalculator
    {
        string ComputeHash(Stream stream);
    }
}
