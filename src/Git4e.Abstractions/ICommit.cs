using System;

namespace Git4e
{
    public interface ICommit : IHashableObject
    {
        string Author { get; set; }
        DateTime When { get; set; }
        string Message { get; set; }
        LazyHashableObjectBase Root { get; set; }
        string[] ParentCommitHashes { get; set; }
    }
}
