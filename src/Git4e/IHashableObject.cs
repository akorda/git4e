using System.IO;

namespace Git4e
{
    public interface IHashableObject
    {
        string Type { get; }
        void SerializeContent(Stream stream, IHashCalculator hashCalculator);
        //void MarkContentAsDirty();
        byte[] ComputeHash(IHashCalculator hashCalculator);
    }
}
