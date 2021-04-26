using System;

namespace Git4e
{
    /// <summary>
    /// Represents a commit to the repository.
    /// </summary>
    public interface ICommit : IHashableObject
    {
        /// <summary>
        /// Commit author.
        /// </summary>
        string Author { get; set; }

        /// <summary>
        /// Time that the commit occured.
        /// </summary>
        DateTime When { get; set; }

        /// <summary>
        /// A short (but probably multiline) message of the commit.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// A lazy hashable object of the repository root.
        /// </summary>
        LazyHashableObjectBase Root { get; set; }

        /// <summary>
        /// Zero, one or more parent commits. Can be null, or an empty array if this
        /// is the initial commit of the repository.
        /// </summary>
        string[] ParentCommitHashes { get; set; }
    }
}
