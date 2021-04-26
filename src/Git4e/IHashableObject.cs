using System.IO;

namespace Git4e
{
    public interface IHashableObject
    {
        string Type { get; }
        void SerializeContent(Stream stream);
        byte[] Hash { get; }
    }
}
