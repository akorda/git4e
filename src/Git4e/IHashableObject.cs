using System.Collections.Generic;
using System.IO;

namespace Git4e
{
    public interface IHashableObject
    {
        string Type { get; }
        void SerializeContent(Stream stream);
        string Hash { get; }
        IEnumerable<IHashableObject> ChildObjects { get; }
    }
}
