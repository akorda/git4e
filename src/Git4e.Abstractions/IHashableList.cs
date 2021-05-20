using System.Collections.Generic;

namespace Git4e
{
    public interface IHashableList// : IEnumerable<IHashableObject>
    {
        string Hash { get; }
        string[] FullHashes { get; }
        void MarkAsDirty();
        IEnumerable<IHashableObject> GetItems();
    }
}
